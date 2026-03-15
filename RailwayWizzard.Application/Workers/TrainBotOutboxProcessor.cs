using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RailwayWizzard.Application.TrainBotTasks;
using RailwayWizzard.Core.TrainBotOutbox;
using RailwayWizzard.Core.TrainBotTasks;
using RailwayWizzard.Infrastructure;
using RailwayWizzard.Infrastructure.Repositories.TrainBotTasks;

namespace RailwayWizzard.Application.Workers
{
    /// <summary>
    /// Воркер для переноса задач из outbox train_bot в таблицу train_bot_tasks.
    /// </summary>
    public class TrainBotOutboxProcessor : BackgroundService
    {
        private const int BatchSize = 20;
        private const int RunIntervalMs = 2000;
        private static readonly TimeSpan StaleProcessingWindow = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TrainBotOutboxProcessor> _logger;

        public TrainBotOutboxProcessor(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<TrainBotOutboxProcessor> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var hadWork = await DoWorkAsync(stoppingToken);
                    if (!hadWork)
                        await Task.Delay(RunIntervalMs, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Worker} failed while processing outbox.", nameof(TrainBotOutboxProcessor));
                    await Task.Delay(RunIntervalMs, stoppingToken);
                }
            }
        }

        private async Task<bool> DoWorkAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RailwayWizzardAppContext>();
            var repository = scope.ServiceProvider.GetRequiredService<ITrainBotTaskRepository>();

            var now = DateTimeOffset.UtcNow;
            var staleBefore = now - StaleProcessingWindow;

            var messages = await context.TrainBotOutboxMessages
                .Where(m => m.Status == "NEW" || (m.Status == "PROCESSING" && m.LockedAt < staleBefore))
                .OrderBy(m => m.CreatedAt)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (messages.Count == 0)
                return false;

            foreach (var message in messages)
            {
                message.Status = "PROCESSING";
                message.LockedAt = now;
                message.Attempts += 1;
            }

            await context.SaveChangesAsync(cancellationToken);

            foreach (var message in messages)
            {
                if (!TrainBotTaskParser.TryParseCreated(message, out var task, out var error))
                {
                    MarkError(message, error);
                    await context.SaveChangesAsync(cancellationToken);
                    continue;
                }

                try
                {
                    var entity = MapToEntity(task, message.Payload);
                    await repository.CreateAsync(entity, cancellationToken);

                    message.Status = "SENT";
                    message.SentAt = DateTimeOffset.UtcNow;
                    message.LastError = null;
                    await context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    _logger.LogWarning(ex,
                        "{Worker} detected duplicate task_id {TaskId} from outbox {OutboxId}.",
                        nameof(TrainBotOutboxProcessor),
                        task.TaskId,
                        message.Id);

                    message.Status = "SENT";
                    message.SentAt = DateTimeOffset.UtcNow;
                    message.LastError = null;
                    await context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "{Worker} failed to process outbox {OutboxId}.",
                        nameof(TrainBotOutboxProcessor),
                        message.Id);

                    MarkError(message, ex.Message);
                    await context.SaveChangesAsync(cancellationToken);
                }
            }

            return true;
        }

        private static void MarkError(TrainBotMessage message, string error)
        {
            message.Status = "ERROR";
            message.LastError = error;
        }

        private static bool IsUniqueViolation(DbUpdateException exception)
        {
            return exception.InnerException is PostgresException pg &&
                   pg.SqlState == PostgresErrorCodes.UniqueViolation;
        }

        private static TrainBotTaskEntity MapToEntity(TrainBotTask task, string payload)
        {
            return new TrainBotTaskEntity
            {
                TaskId = task.TaskId,
                UserId = task.UserId,
                TelegramId = task.TelegramId,
                From = task.From,
                To = task.To,
                Date = task.Date,
                Seats = task.Seats,
                Mode = MapMode(task.Mode),
                SeatMode = MapSeatMode(task.SeatMode),
                AccessibleOnly = task.AccessibleOnly,
                ModeParams = BuildModeParams(task),
                SeatParams = BuildSeatParams(task),
                Payload = payload,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private static string MapMode(TrainBotModeType mode) => mode switch
        {
            TrainBotModeType.TimeInterval => "time_interval",
            _ => "specific_train"
        };

        private static string MapSeatMode(TrainBotSeatMode mode) => mode switch
        {
            TrainBotSeatMode.AnySeat => "any_seat",
            TrainBotSeatMode.SpecificCar => "specific_car",
            _ => "by_service_class"
        };

        private static string BuildModeParams(TrainBotTask task)
        {
            switch (task.Mode)
            {
                case TrainBotModeType.SpecificTrain:
                {
                    var mode = (SpecificTrainModeParams)task.ModeParams;
                    return JsonSerializer.Serialize(new
                    {
                        time = mode.Time.ToString("HH:mm")
                    });
                }
                case TrainBotModeType.TimeInterval:
                {
                    var mode = (TimeIntervalModeParams)task.ModeParams;
                    return JsonSerializer.Serialize(new
                    {
                        dep_from = FormatTime(mode.DepFrom),
                        dep_to = FormatTime(mode.DepTo),
                        arr_from = FormatTime(mode.ArrFrom),
                        arr_to = FormatTime(mode.ArrTo)
                    });
                }
                default:
                    return "{}";
            }
        }

        private static string BuildSeatParams(TrainBotTask task)
        {
            switch (task.SeatMode)
            {
                case TrainBotSeatMode.AnySeat:
                    return JsonSerializer.Serialize(new { });
                case TrainBotSeatMode.ByServiceClass:
                {
                    var seat = (ByServiceClassParams)task.SeatParams;
                    return JsonSerializer.Serialize(new
                    {
                        classes = seat.Classes
                    });
                }
                case TrainBotSeatMode.SpecificCar:
                {
                    var seat = (SpecificCarParams)task.SeatParams;
                    return JsonSerializer.Serialize(new
                    {
                        wagon = seat.Wagon
                    });
                }
                default:
                    return "{}";
            }
        }

        private static string? FormatTime(TimeOnly? time) =>
            time?.ToString("HH:mm");
    }
}
