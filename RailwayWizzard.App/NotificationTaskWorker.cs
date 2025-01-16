using RailwayWizzard.B2BHelper.App;
using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App
{
    public class NotificationTaskWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 60;

        private readonly ISteps _steps;
        private readonly INotificationTaskRepository _notificationTaskRepository;
        private readonly ILogger<NotificationTaskWorker> _logger;

        public NotificationTaskWorker(
            ISteps steps,
            INotificationTaskRepository notificationTaskRepository,
            ILogger<NotificationTaskWorker> logger)
        {
            _steps = steps;
            _logger = logger;
            _notificationTaskRepository = notificationTaskRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(NotificationTaskWorker)} running at: {Common.MoscowNow} Moscow time");

                await DoWork(cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            if (Common.IsDownTimeRzd())
            {
                await Task.Delay(RUN_INTERVAL, cancellationToken);
                _logger.LogInformation($"{nameof(NotificationTaskWorker)} RZD DownTime. Today:{Common.MoscowNow}");
                return;
            }

            try
            {
                var notificationTask = await _notificationTaskRepository.GetOldestAsync();

                if (notificationTask is null)
                {
                    await Task.Delay(RUN_INTERVAL, cancellationToken);
                    _logger.LogInformation($"{nameof(NotificationTaskWorker)} NotificationTask is null. Run Delay. Today:{Common.MoscowNow}");
                    return;
                }

                await _steps.Notification(notificationTask);
            }

            catch (Exception ex)
            {
                _logger.LogError($"{nameof(NotificationTaskWorker)} {ex}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(NotificationTaskWorker)} stopped at: {Common.MoscowNow} Moscow time");

            await base.StopAsync(cancellationToken);
        }
    }
}