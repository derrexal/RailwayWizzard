using RailwayWizzard.Shared;

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
            _nameWorker = GetType().Name.ToString();
            _runningInterval = runningInterval;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var todayTime = TimeOnly.FromDateTime(Common.GetMoscowDateTime);
            
            // Сервис РЖД проводит тех. работы
            var IsDownTime = todayTime > _startDownTime && todayTime < _endDownTime;

            _logger.LogInformation($"DEBUG DownTimeCheck todayTime:{todayTime} IsDownTime{IsDownTime}");

            while (!cancellationToken.IsCancellationRequested && !IsDownTime)
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