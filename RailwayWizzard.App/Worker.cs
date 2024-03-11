using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Controllers;
using RailwayWizzard.App.Data;
using RailwayWizzard.Core;
using RzdHack.Robot.App;
using RzdHack.Robot.Core;
using System.Globalization;



namespace RailwayWizzard.App
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly ILogger<StationInfoController> _loggerStationInfoController;
        private readonly IDbContextFactory<RailwayWizzardAppContext> _contextFactory;
        public Worker(ILogger<Worker> logger, ILogger<StationInfoController> _loggerStationInfoController, IDbContextFactory<RailwayWizzardAppContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await DoWork(cancellationToken);
                //Запускается 1 раз в 16 минут
                await Task.Delay(1000*60*16, cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            try
            {
                await UpdateIsActual();

                var activeNotificationTasks = await GetActive();

                foreach (var task in activeNotificationTasks)
                {
                    using (var _context = _contextFactory.CreateDbContext())
                    {
                        //Дозаполняем кода городов
                        task.ArrivalStationCode =
                            (await new StationInfoController(_context, _loggerStationInfoController).GetByName(
                                new StationInfo { StationName = task.ArrivalStation })).ExpressCode;
                        task.DepartureStationCode =
                            (await new StationInfoController(_context, _loggerStationInfoController).GetByName(
                                new StationInfo { StationName = task.DepartureStation })).ExpressCode;
                    }
                    //Инициализируем задачу в новом потоке
                    var t = new Thread(() => new StepsUsingHttpClient(_logger).Notification(task));
                    //Запуск задачи
                    t.Start();
                    _logger.LogInformation($"Run Task:{task.Id} in Thread:{t.ManagedThreadId}");

                }
            }
            catch 
            { throw; }
        }

        /// <summary>
        /// Обновляет поле "IsActual" если поездка уже в прошлом
        /// </summary>
        /// <returns></returns>
        private async Task UpdateIsActual()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var activeNotificationTasks = await GetActive();
                foreach (var activeNotificationTask in activeNotificationTasks)
                {
                    DateTime itemDateFromDateTime = DateTime.ParseExact(
                        activeNotificationTask.DateFrom.ToShortDateString() + " " + activeNotificationTask.TimeFrom,
                        //"dd.MM.yyyy HH:mm",
                        "MM/dd/yyyy HH:mm",
                        CultureInfo.InvariantCulture);

                    if (itemDateFromDateTime < DateTime.Now)
                    {
                        activeNotificationTask.IsActual = false;
                        _context.NotificationTask.Update(activeNotificationTask);
                    }
                }
                await _context.SaveChangesAsync();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Получает список задач со статусом "Актуально"
        /// </summary>
        /// <returns></returns>
        private async Task<IList<NotificationTask>> GetActive()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var notificationTasks = await _context.NotificationTask
                    .Where(t => t.IsActual == true)
                    .ToListAsync();
                return notificationTasks;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Worker stopped at: {DateTimeOffset.Now}");
            await base.StopAsync(cancellationToken);
        }


        public override void Dispose()
        {
            base.Dispose();
        }
    }
}   