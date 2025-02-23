using RailwayWizzard.Common;
using RailwayWizzard.Core.MessageOutbox;
using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Infrastructure.Repositories.MessagesOutbox;
using RailwayWizzard.Rzd.DataEngine.Services;

namespace RailwayWizzard.Application.Workers
{
    /// <summary>
    /// Периодически проверяет работоспособность сервиса получения информации от РЖД
    /// </summary>
    public class HealthCheckWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 60 * 3; // Интервал запуска (3 мин)
        private const int MAX_RUN_TIME = 1000 * 30; // Допустимое время выполнения проверки (30 сек)

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HealthCheckWorker> _logger;

        public HealthCheckWorker(
            IServiceProvider serviceProvider,
            ILogger<HealthCheckWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (DateTimeExtensions.IsDownTimeRzd())
                {
                    _logger.LogInformation($"{nameof(NotificationTaskWorker)} RZD DownTime. Today:{DateTimeExtensions.MoscowNow}");
                    await Task.Delay(RUN_INTERVAL, cancellationToken);
                    return;
                }

                _logger.LogInformation($"{nameof(HealthCheckWorker)} running at: {DateTimeExtensions.MoscowNow} Moscow time");

                await DoWork();

                await Task.Delay(RUN_INTERVAL, cancellationToken);
            }
        }

        private async Task DoWork()
        {
            // TODO: Это работает пока у нас имеются записи в базе. Если с нуля разворачивать бота без проиниализированных значений - поверка помрет.
            // Изменить input dto в FindFreeSeatsAsync и передавать туда ExpressCode который статичен
            var testNotificationTask = new NotificationTask
            {
                DepartureStationId = 5, // Москва
                ArrivalStationId = 159,   // Орел
                DepartureDateTime = DateTime.Today.AddDays(30).AddHours(8).AddMinutes(46), // 08:46
                CarTypes = new List<CarType> { CarType.Sedentary, CarType.SedentaryBusiness}
            };

            var baseMessage = $"[{nameof(HealthCheckWorker)}] Время:{DateTimeExtensions.MoscowNow} Рейс {testNotificationTask.ToLogString()} ";

            using var scope = _serviceProvider.CreateScope();
            var dataExtractor = scope.ServiceProvider.GetRequiredService<IDataExtractor>();
            var messageOutboxRepository = scope.ServiceProvider.GetRequiredService<IMessageOutboxRepository>();

            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                var freeSeatsText = await dataExtractor.FindFreeSeatsAsync(testNotificationTask);

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

                var message = new MessageOutbox
                {
                    NotificationTaskId = 999999,
                    Message = messageError,
                    Created = DateTime.Now,
                    UserId = BussinesConstants.ADMIN_USER_ID
                };
                await messageOutboxRepository.CreateAsync(message);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(HealthCheckWorker)} stopped at: {DateTimeExtensions.MoscowNow} Moscow time");

            await base.StopAsync(cancellationToken);
        }
    }
}