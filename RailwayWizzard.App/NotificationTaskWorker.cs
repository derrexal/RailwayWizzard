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
        private readonly ILogger _logger;
        private readonly IDbContextFactory<RailwayWizzardAppContext> _contextFactory;
        private const int timeInterval = 1000 * 60 * 10; //»нтервал запуска (10 мин)

        public NotificationTaskWorker(
            IRobot robot,
            IBotApi botApi,
            IChecker checker,
            ILogger<NotificationTaskWorker> logger, 
            IDbContextFactory<RailwayWizzardAppContext> contextFactory)
        {
            _robot = robot;
            _botApi = botApi;
            _checker = checker;
            _logger = logger;
            _contextFactory = contextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await DoWork();
                //TODO: ¬се-таки хочетс€, чтобы работа по задаче началась непосредственно после создани€

                await Task.Delay(timeInterval, cancellationToken);
            }
        }

        private async Task DoWork()
        {
            try
            {
                var currentNotificationTasks = await _checker.GetNotificationTasksForWork();
                foreach (var task in currentNotificationTasks)
                {
                    new StepsUsingHttpClient(_robot,_botApi,_checker,_logger, _contextFactory).Notification(task);
                    _logger.LogTrace($"Run Task:{task.Id} in Thread:{Thread.CurrentThread.ManagedThreadId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Worker {ex}");
                throw; 
            }
            
        }
        
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Worker stopped at: {DateTimeOffset.Now}");
            await base.StopAsync(cancellationToken);
        }
    }
}   