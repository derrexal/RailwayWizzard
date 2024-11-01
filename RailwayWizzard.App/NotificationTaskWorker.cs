using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.Robot.App;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App
{
    public class NotificationTaskWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 60; //Интервал запуска (1 мин)

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
            try
            {
                await _notificationTaskRepository.DatabaseInitialize();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(NotificationTaskWorker)} {ex}");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(NotificationTaskWorker)} running at: {Common.MoscowNow} Moscow time");

                await DoWork(cancellationToken);
            }
        }

        protected async Task DoWork(CancellationToken cancellationToken)
        {
            var isDownTime = Common.IsDownTimeRzd();
            if (isDownTime)
            {
                await Task.Delay(RUN_INTERVAL, cancellationToken);
                return;
            }

            try
            {
                var notificationTask = await _notificationTaskRepository.GetOldestNotificationTask();

                if (notificationTask is null)
                {
                    //TODO: допилить - увеличивать время если задач нет до определенного максимума как в проекте с прошлого места.
                    await Task.Delay(RUN_INTERVAL, cancellationToken);
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