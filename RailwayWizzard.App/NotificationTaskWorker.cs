using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.Robot.App;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App
{
    public class NotificationTaskWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 10; //Интервал запуска (10 сек)

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
                _logger.LogInformation($"{nameof(NotificationTaskWorker)} running at: {Common.GetMoscowDateTime} Moscow time");

                await DoWork();

                await Task.Delay(RUN_INTERVAL, cancellationToken);
            }
        }

        protected async Task DoWork()
        {
            var isDownTime = Common.IsDownTimeRzd();
            if (isDownTime) return;

            // TODO: отказался от этой идеи из-за проблем с контекстом. (System.InvalidOperationException: A second operation was started on this context instance before a previous operation completed. This is usually caused by different threads concurrently using the same instance of DbContext. For more information on how to avoid threading issues with DbContext)
            //List<Task> tasks = new();

            try
            {
                await _notificationTaskRepository.DatabaseInitialize();

                var notificationTasks = await _notificationTaskRepository.GetNotificationTasksForWork();

                foreach (var notificationTask in notificationTasks)
                    await _steps.Notification(notificationTask);

                //await Task.WhenAll(tasks);
            }

            catch (Exception ex)
            {
                _logger.LogError($"{nameof(NotificationTaskWorker)} {ex}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(NotificationTaskWorker)} stopped at: {Common.GetMoscowDateTime} Moscow time");

            await base.StopAsync(cancellationToken);
        }
    }
}