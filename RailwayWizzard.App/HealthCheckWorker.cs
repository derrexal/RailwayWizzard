using RailwayWizzard.Core;
using RailwayWizzard.Robot.App;

namespace RailwayWizzard.App
{
    /// <summary>
    /// Периодически проверяет работоспособность сервиса получения информации от РЖД
    /// </summary>
    public class HealthCheckWorker : BaseRaiwayWizzardBackgroundService
    {
        private const int runningInterval = 1000 * 60 * 10; // Интервал запуска (10 мин)
        private const int maxTime = 30000; // Допустимое время выполнения проверки

        private readonly ILogger _logger;
        private readonly IBotApi _botApi;
        private readonly IRobot _robot;
        
        public HealthCheckWorker(
            ILogger<HealthCheckWorker> logger, 
            IBotApi botApi, 
            IRobot robot)
            : base(runningInterval, logger)
        {
            _botApi = botApi;
            _logger = logger;
            _robot = robot;
        }

        protected override async Task DoWork()
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

                string message = "";
                if (executionTime > maxTime)
                {
                    message = $"[{this.GetType().Name}] Рейс {testNotificationTask.ToCustomString()} " +
                        $"превышено допустимое время выполнения({maxTime/ 1000} с): {executionTime} мс";
                    _logger.LogInformation(message);
                    await _botApi.SendMessageForAdminAsync(message);
                }
                //TODO: вынести в Exceptions
                if (freeSeats.Count==0)
                {
                    message += $"[{this.GetType().Name}] Рейс {testNotificationTask.ToCustomString()} не обнаружено свободных мест." +
                        $"Время выполнения метода: {executionTime} мс";
                    _logger.LogInformation(message);
                }
                if (message != "") return;

                message = $"[{this.GetType().Name}] Рейс {testNotificationTask.ToCustomString()} проверка выполнена успешно." +
                    $"Время выполнения метода: {executionTime} мс";
                _logger.LogTrace(message);
            }
            catch (Exception ex)
            {
                string messageError = $"[{this.GetType().Name}] Рейс {testNotificationTask.ToCustomString()} возникла ошибка: {ex.Message}";
                _logger.LogError(messageError);
                await _botApi.SendMessageForAdminAsync(messageError);
                throw; 
            }
        }
    }
}