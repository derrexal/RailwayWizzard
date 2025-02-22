using RailwayWizzard.Common;
using RailwayWizzard.Engine.Services;
using RailwayWizzard.Infrastructure.Repositories.NotificationTasks;
using RailwayWizzard.Infrastructure.Repositories.Users;

namespace RailwayWizzard.Application.Workers
{
    public class NotificationTaskWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 60;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationTaskWorker> _logger;

        public NotificationTaskWorker(
        IServiceProvider serviceProvider,
        ILogger<NotificationTaskWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (DateTimeExtensions.IsDownTimeRzd())
                {
                    _logger.LogInformation($"{nameof(NotificationTaskWorker)} RZD DownTime. Today:{DateTimeExtensions.MoscowNow}");
                    await Task.Delay(RUN_INTERVAL, cancellationToken);
                    return;
                }

                _logger.LogInformation($"{nameof(NotificationTaskWorker)} running at: {DateTimeExtensions.MoscowNow} Moscow time");

                await DoWork(cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationTaskRepository = scope.ServiceProvider.GetRequiredService<INotificationTaskRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();

            try
            {
                var notificationTask = await notificationTaskRepository.GetOldestAsync();

                if (notificationTask is null)
                {
                    _logger.LogInformation($"{nameof(NotificationTaskWorker)} No tasks found for execution. " +
                                           $"Process delay by {RUN_INTERVAL} ms. Today:{DateTimeExtensions.MoscowNow}");
                    await Task.Delay(RUN_INTERVAL, cancellationToken);
                    return;
                }

                var user = await userRepository.GetUserByIdAsync(notificationTask.CreatorId);

                if (user.HasBlockedBot)
                {
                    _logger.LogInformation(
                        $"{nameof(NotificationTaskWorker)} Task Id {notificationTask.Id} is not run process. " +
                        $"User blocked bot. Today:{DateTimeExtensions.MoscowNow}");
                    return;
                }

                await dataProcessor.RunProcessTaskAsync(notificationTask);
            }

            catch (Exception ex)
            {
                _logger.LogError($"{nameof(NotificationTaskWorker)} {ex}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(NotificationTaskWorker)} stopped at: {DateTimeExtensions.MoscowNow} Moscow time");

            await base.StopAsync(cancellationToken);
        }
    }
}