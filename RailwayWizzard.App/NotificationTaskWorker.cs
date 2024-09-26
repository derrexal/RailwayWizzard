using RailwayWizzard.EntityFrameworkCore.UnitOfWork;
using RailwayWizzard.Robot.App;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App
{
    public class NotificationTaskWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 60 * 1; //Интервал запуска (1 мин)

        private readonly ISteps _steps;
        private readonly IRailwayWizzardUnitOfWork _uow;
        private readonly ILogger<NotificationTaskWorker> _logger;

        public NotificationTaskWorker(
            ISteps steps,
            IRailwayWizzardUnitOfWork uow,
            ILogger<NotificationTaskWorker> logger) 
        {
            _uow = uow;
            _logger = logger;
            _steps = steps;
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

            List<Task> tasks = new();
            
            try
            {
                var notificationTasks = await _uow.NotificationTaskRepository.GetNotificationTasksForWork();
                
                foreach (var notificationTask in notificationTasks)
                    tasks.Add(_steps.Notification(notificationTask));
                
                await Task.WhenAll(tasks);
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