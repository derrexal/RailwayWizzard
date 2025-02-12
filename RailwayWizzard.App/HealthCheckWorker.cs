﻿using RailwayWizzard.B2BHelper.App;
using RailwayWizzard.Core;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App
{
    /// <summary>
    /// Периодически проверяет работоспособность сервиса получения информации от РЖД
    /// </summary>
    public class HealthCheckWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 60 * 10; // Интервал запуска (10 мин)
        private const int MAX_RUN_TIME = 30000; // Допустимое время выполнения проверки

        private readonly ILogger _logger;
        private readonly IBotClient _botApi;
        private readonly IRobot _robot;

        public HealthCheckWorker(
            ILogger<HealthCheckWorker> logger,
            IBotClient botApi,
            IRobot robot)
        {
            _botApi = botApi;
            _logger = logger;
            _robot = robot;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(HealthCheckWorker)} running at: {Common.MoscowNow} Moscow time");

                await DoWork();

                await Task.Delay(RUN_INTERVAL, cancellationToken);
            }
        }

        private async Task DoWork()
        {
            if (Common.IsDownTimeRzd()) 
                return;

            // TODO: вынести в моки.
            var testNotificationTask = new NotificationTask
            {
                DepartureStation = "Москва",
                ArrivalStation = "Орел",
                DepartureStationCode = 2000000,
                ArrivalStationCode = 2000140,
                DepartureDateTime = DateTime.Now.AddDays(30).AddHours(8).AddMinutes(46), // today 08:46
                CarTypes = new List<CarTypeEnum> { CarTypeEnum.Sedentary, CarTypeEnum.ReservedSeatLower, CarTypeEnum.CompartmentLower, CarTypeEnum.Luxury }
            };

            var baseMessage = $"[{nameof(HealthCheckWorker)}] Время:{Common.MoscowNow} Рейс {testNotificationTask.ToCustomString()} ";

            try
            {
                //Время выполнения метода
                var watch = System.Diagnostics.Stopwatch.StartNew();

                var freeSeatsText = await _robot.GetFreeSeatsOnTheTrain(testNotificationTask);

                watch.Stop();

                var executionTime = watch.ElapsedMilliseconds;

                if (executionTime > MAX_RUN_TIME)
                    _logger.LogWarning(baseMessage + $"превышено допустимое время выполнения({MAX_RUN_TIME} мс): {executionTime} мс");
                else if (freeSeatsText == string.Empty)
                    _logger.LogWarning(baseMessage + $"не обнаружено свободных мест. Время выполнения метода: {executionTime} мс");
                else
                    _logger.LogInformation(baseMessage + $"проверка выполнена успешно. Время выполнения метода: {executionTime} мс");    
            }
            
            catch (Exception ex)
            {
                var messageError = baseMessage + $"возникла ошибка: {ex.Message}";
                _logger.LogError(messageError);
                await _botApi.SendMessageForAdminAsync(messageError);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(HealthCheckWorker)} stopped at: {Common.MoscowNow} Moscow time");

            await base.StopAsync(cancellationToken);
        }
    }
}