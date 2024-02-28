using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Controllers;
using RailwayWizzard.App.Data;
using RailwayWizzard.Core;
using RzdHack.Robot.App;
using RzdHack.Robot.Core;
using System.Collections.Generic;
using System.Globalization;


namespace RailwayWizzard.App
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IDbContextFactory<RailwayWizzardAppContext> _contextFactory;
        private static readonly Dictionary<int, int> _taskDictionary = new();
        public Worker(ILogger<Worker> logger, IDbContextFactory<RailwayWizzardAppContext> contextFactory)
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
                            (await new StationInfoController(_context).GetByName(
                                new StationInfo { StationName = task.ArrivalStation })).ExpressCode;
                        task.DepartureStationCode =
                            (await new StationInfoController(_context).GetByName(
                                new StationInfo { StationName = task.DepartureStation })).ExpressCode;
                    }

                    //Запускаем задачу новым потоком 
                    var t = new Thread(() => new StepsUsingHttpClient().Notification(task));
                    try
                    {
                        t.Start();
                        //Добавляем в коллекцию номер задачи-номер потока
                        _taskDictionary.Add(task.Id, t.ManagedThreadId);
                        _logger.LogInformation($"Запущена задача номер: {task.Id} в потоке номер: {t.ManagedThreadId}");

                    }
                    catch
                    {
                        //Если задача вызвала ошибку - удаляем ее из списка активных задач и гасим поток
                        if (_taskDictionary.ContainsValue(t.ManagedThreadId))
                        {
                            var item = _taskDictionary.FirstOrDefault(task => task.Value == t.ManagedThreadId);
                            _taskDictionary.Remove(item.Key);
                            t.Abort();
                            //Это погасит приложение? А надо ли?
                            //throw;
                        }
                    }
                }
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