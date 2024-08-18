using RailwayWizzard.Core;
using RailwayWizzard.Robot.App;

namespace RailwayWizzard.App
{
    /// <summary>
    /// Периодически проверяет работоспособность сервиса получения информации от РЖД
    /// </summary>
    public class HealthCheckWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IBotApi _botApi;
        private readonly IRobot _robot;
        private const int timeInterval = 1000 * 60 * 10; //Интервал запуска (10 мин)
        
        public HealthCheckWorker(ILogger<HealthCheckWorker> logger, IBotApi botApi, IRobot robot)
        {
            _botApi = botApi;
            _logger = logger;
            _robot = robot;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(HealthCheckWorker)} running at: {DateTimeOffset.Now}");

                await DoWork();

                await Task.Delay(timeInterval, cancellationToken);
            }
        }

        private async Task DoWork()
        {
            // тестовые данные
            NotificationTask testNotificationTask = new NotificationTask
            {
                DepartureStation = "Москва",
                ArrivalStation = "Орел",
                DepartureStationCode = 2000000,
                ArrivalStationCode = 2000140,
                TimeFrom = "08:46",
                DateFrom = DateTime.Today.AddDays(30),
                CarTypes = new List<CarTypeEnum>{CarTypeEnum.Sedentary, CarTypeEnum.ReservedSeat, CarTypeEnum.Compartment, CarTypeEnum.Luxury}
            };
            try
            {
                //Время выполнения метода
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var freeSeats = await _robot.GetFreeSeatsOnTheTrain(testNotificationTask);
                watch.Stop();
                var executionTime = watch.ElapsedMilliseconds;

                string message;
                if (executionTime > 30000)
                {
                    message = $"[{this.GetType().Name}] В ходе проверки доступности на примере рейса {testNotificationTask.ToCustomString()} \n" +
                        $"превышено допустимое время выполнения(30 с): {executionTime} мс";
                    _logger.LogInformation(message);
                    await _botApi.SendMessageForAdminAsync(message);
                }

                if (freeSeats.Count==0)
                {
                    message = $"[{this.GetType().Name}] В ходе проверки доступности на примере рейса {testNotificationTask.ToCustomString()}\n не обнаружено свободных мест\n\n" +
                        $"Дополнительная информация: время выполнения метода: {executionTime} мс";
                    _logger.LogInformation(message);
                    await _botApi.SendMessageForAdminAsync(message);
                }
            }
            catch (Exception ex)
            {
                string messageError = $"[{this.GetType().Name}] В ходе проверки доступности на примере рейса {testNotificationTask.ToCustomString()}\nвозникла ошибка: {ex.Message}";
                await _botApi.SendMessageForAdminAsync(messageError);
                _logger.LogError(messageError);
                throw; 
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(HealthCheckWorker)} stopped at: {DateTimeOffset.Now}");
            await base.StopAsync(cancellationToken);
        }
    }
}