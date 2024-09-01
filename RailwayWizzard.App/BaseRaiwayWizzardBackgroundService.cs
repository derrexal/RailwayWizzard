namespace RailwayWizzard.App
{
    /// <summary>
    /// Воркер который не работает в период с 03:20 по 04:10
    /// </summary>
    public abstract class BaseRaiwayWizzardBackgroundService : BackgroundService
    {
        private readonly TimeOnly _startDownTime = new TimeOnly(03,20);
        private readonly TimeOnly _endDownTime = new TimeOnly(04,10);
        
        private readonly string _nameWorker;
        private readonly int _runningInterval;
        private readonly ILogger _logger;

        public BaseRaiwayWizzardBackgroundService(int runningInterval, ILogger logger)
        {
            _nameWorker = GetType().BaseType.ToString();
            _runningInterval = runningInterval;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var todayTime = TimeOnly.FromDateTime(DateTime.Now);
            
            while (!cancellationToken.IsCancellationRequested && 
                todayTime < _startDownTime && 
                todayTime > _endDownTime)
            {
                _logger.LogInformation($"{_nameWorker} running at: {DateTimeOffset.Now}");

                await DoWork();

                await Task.Delay(_runningInterval, cancellationToken);
            }
        }

        protected virtual Task DoWork()
        {
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{_nameWorker} stopped at: {DateTimeOffset.Now}");
            await base.StopAsync(cancellationToken);
        }
    }
}