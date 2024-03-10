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
        private static readonly Dictionary<int, int> _taskDictionary = new();
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
                //Запускается 1 раз в 15 минут
                await Task.Delay(1000*60*15, cancellationToken);
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
                    //Если такая задача еще не запущена
                    if (!_taskDictionary.ContainsKey(task.Id))
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
                        //Добавление в коллекцию номера задачи и номер потока, в котором эта задача запущена
                        _taskDictionary.Add(task.Id, t.ManagedThreadId);
                        _logger.LogInformation($"Run Task:{task.Id} in Thread:{t.ManagedThreadId} Count Tasks: {_taskDictionary.Count}");

                    }
                }

            }
            catch 
            {
                //Если задача вызвала ошибку - удаляем ее из списка активных задач и гасим поток
                var currentThread = Thread.CurrentThread;
                if (_taskDictionary.ContainsValue(currentThread.ManagedThreadId))
                {
                    var item = _taskDictionary.FirstOrDefault(task => task.Value == c.ManagedThreadId);
                    _taskDictionary.Remove(item.Key);
                    _logger.LogInformation($"Remove task: {item.Key}");
                }
                _logger.LogInformation($"Stopping Thread:{currentThread.ManagedThreadId} Count Tasks: {_taskDictionary.Count}");
                currentThread.Abort();
                _logger.LogInformation($"Abort Thread: {currentThread.ManagedThreadId}");
                throw;
            }
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
            _logger.LogInformation("Worker stopped\n");
            await base.StopAsync(cancellationToken);
        }


        public override void Dispose()
        {
            _taskDictionary.Clear();
            base.Dispose();
        }
    }
}   