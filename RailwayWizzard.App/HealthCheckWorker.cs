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
        private const int timeInterval = 1000 * 60 * 20; //Интервал запуска (20 мин)
        
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
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await DoWork();

                await Task.Delay(timeInterval, cancellationToken);
            }
        }

        private async Task DoWork()
        {
            // тестовые данные
            NotificationTask testNotificationTask = new NotificationTask
            {
                DepartureStationCode = 2000000,
                ArrivalStationCode = 2000140,
                TimeFrom = "05:18",
                DateFrom = DateTime.Today.AddDays(30),
                CarTypes = new List<CarTypeEnum>{CarTypeEnum.Sedentary, CarTypeEnum.ReservedSeat, CarTypeEnum.Compartment, CarTypeEnum.Luxury}
            };
            string message;
            try
            {
                //Время выполнения метода
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var freeSeats = await _robot.GetFreeSeatsOnTheTrain(testNotificationTask);
                watch.Stop();
                var executionTime = watch.ElapsedMilliseconds;

                if (executionTime > 15000)
                {
                    message = $"[{this.GetType().Name}] В ходе проверки доступности на примере рейса {testNotificationTask.ToCustomString()} \nпревышено время выполнения: {executionTime} мс";
                    _logger.LogInformation(message);
                    await _botApi.SendMessageForAdminAsync(message);
                }

                if (freeSeats.Count == 0)
                {
                    message = $"[{this.GetType().Name}] В ходе проверки доступности не обнаружено свободных мест на рейс: \n{testNotificationTask.ToCustomString()}\n\nВремя выполнения метода: {executionTime} мс";
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
    }
}