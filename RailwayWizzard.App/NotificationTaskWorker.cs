using RailwayWizzard.EntityFrameworkCore.UnitOfWork;
using RailwayWizzard.Robot.App;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App
{
    public class NotificationTaskWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 60 * 1; //Интервал запуска (1 мин)

        private readonly IRobot _robot;
        private readonly IBotApi _botApi;
        private readonly IRailwayWizzardUnitOfWork _uow;
        private readonly ILogger<NotificationTaskWorker> _logger;
        private readonly ILogger<StepsUsingHttpClient> _stepsLogger;
        private readonly ISteps _steps;

        public NotificationTaskWorker(
            IRobot robot,
            IBotApi botApi,
            IRailwayWizzardUnitOfWork uow,
            ILogger<NotificationTaskWorker> logger, 
            ILogger<StepsUsingHttpClient> stepsLogger) 
        {
            _robot = robot;
            _botApi = botApi;
            _uow = uow;
            _logger = logger;
            _stepsLogger = stepsLogger;
            _steps = new StepsUsingHttpClient(_robot, _botApi, _uow, _stepsLogger);
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