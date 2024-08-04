using RailwayWizzard.Robot.App;
using RailwayWizzard.Shared;


namespace RailwayWizzard.App
{
    public class NotificationTaskWorker : BackgroundService
    {
        private readonly IRobot _robot;
        private readonly IBotApi _botApi;
        private readonly IChecker _checker;
        private readonly ILogger<NotificationTaskWorker> _logger;
        private readonly ILogger<StepsUsingHttpClient> _stepsLogger;
        private readonly ISteps _steps;
        private const int runningInterval = 1000 * 60 * 1; //Интервал запуска (1 мин)

        public NotificationTaskWorker(
            IRobot robot,
            IBotApi botApi,
            IChecker checker,
            ILogger<NotificationTaskWorker> logger, 
            ILogger<StepsUsingHttpClient> stepsLogger)
        {
            _robot = robot;
            _botApi = botApi;
            _checker = checker;
            _logger = logger;
            _stepsLogger = stepsLogger;
            _steps = new StepsUsingHttpClient(_robot, _botApi, _checker, _stepsLogger);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(NotificationTaskWorker)} running at: {DateTimeOffset.Now}");

                await DoWork();
                //TODO: Все-таки хочется, чтобы работа по задаче началась непосредственно после создания

                await Task.Delay(runningInterval, cancellationToken);
            }
        }

        private async Task DoWork()
        {
            List<Task> tasks = new();
            try
            {
                var notificationTasks = await _checker.GetNotificationTasksForWork();
                foreach (var notificationTask in notificationTasks)
                    // TODO: запустив эту задачу мы не запустим другие. Как их запустить скопом?
                    tasks.Add(_steps.Notification(notificationTask));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(NotificationTaskWorker)} {ex}");
                throw; 
            }
            
        }
        
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(NotificationTaskWorker)} stopped at: {DateTimeOffset.Now}");
            await base.StopAsync(cancellationToken);
        }
    }
}