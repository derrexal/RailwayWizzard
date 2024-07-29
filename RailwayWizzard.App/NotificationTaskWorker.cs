using Microsoft.EntityFrameworkCore;
using RailwayWizzard.EntityFrameworkCore.Data;
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
        private const int timeInterval = 1000 * 60 * 10; //Интервал запуска (10 мин)

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
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(NotificationTaskWorker)} running at: {DateTimeOffset.Now}");

                await DoWork();
                //TODO: Все-таки хочется, чтобы работа по задаче началась непосредственно после создания

                await Task.Delay(timeInterval, cancellationToken);
            }
        }

        //TODO: переделать это безобразие
        private async Task DoWork()
        {
            try
            {
                var currentNotificationTasks = await _checker.GetNotificationTasksForWork();
                foreach (var task in currentNotificationTasks)
                {
                    new StepsUsingHttpClient(_robot,_botApi,_checker, _stepsLogger).Notification(task);
                    _logger.LogTrace($"Run Task:{task.Id} in Thread:{Thread.CurrentThread.ManagedThreadId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {nameof(NotificationTaskWorker)} {ex}");
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