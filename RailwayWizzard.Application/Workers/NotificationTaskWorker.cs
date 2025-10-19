using RailwayWizzard.Common;
using RailwayWizzard.Engine.Services;
using RailwayWizzard.Infrastructure.Repositories.NotificationTasks;

namespace RailwayWizzard.Application.Workers
{
    public class NotificationTaskWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 60; // 1 min
        private const int DOWN_TIME_INTERVAL = 1000 * 60 * 30; // 30 mim

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NotificationTaskWorker> _logger;

        public NotificationTaskWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<NotificationTaskWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (DateTimeExtensions.IsDownTimeRzd())
                {
                    _logger.LogInformation($"{nameof(NotificationTaskWorker)} RZD DownTime. Today:{DateTimeExtensions.MoscowNow}");
                    await Task.Delay(DOWN_TIME_INTERVAL, cancellationToken);
                    continue;
                }

                _logger.LogInformation($"{nameof(NotificationTaskWorker)} running at: {DateTimeExtensions.MoscowNow} Moscow time");

                await DoWork(cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var notificationTaskRepository = scope.ServiceProvider.GetRequiredService<INotificationTaskRepository>();

            try
            {
                var notificationTask = await notificationTaskRepository.FindOldestAsync(); 
                if (notificationTask is null)
                {
                    _logger.LogInformation($"{nameof(NotificationTaskWorker)} No tasks found for execution. " +
                                           $"Process delay by {RUN_INTERVAL} ms. Today:{DateTimeExtensions.MoscowNow}");
                    // почему этот Delay оказался здесь, а не в ExecuteAsync?
                    await Task.Delay(RUN_INTERVAL, cancellationToken);
                    return;
                }

                // IDataProcessor можно резолвить перед необходимостью
                await scope
                    .ServiceProvider
                    .GetRequiredService<IDataProcessor>()
                    .RunProcessTaskAsync(notificationTask);
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