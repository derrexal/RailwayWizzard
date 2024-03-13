using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Controllers;
using RailwayWizzard.App.Data;
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
                //TODO: Все-таки хочется, чтобы работа по задаче началась непосредственно после создания
                //Запускается 1 раз в 15 минут
                await Task.Delay(1000*60*15, cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            try
            {
                await UpdateActualStatusNotificationTask();

                var notWorkedNotificationTasks = await GetNotWorkedNotificationTasks();

                foreach (var task in notWorkedNotificationTasks)
                {
                    try
                    {
                        using (var _context = _contextFactory.CreateDbContext())
                        {
                            //TODO: Вынести наполнение в другое место?
                            //Дозаполняем кода городов
                            task.ArrivalStationCode = (await _context.StationInfo.FirstOrDefaultAsync(s => s.StationName == task.ArrivalStation))!.ExpressCode;
                            task.ArrivalStationCode = (await _context.StationInfo.FirstOrDefaultAsync(s => s.StationName == task.DepartureStation))!.ExpressCode;
                        }
                        new StepsUsingHttpClient(_logger).Notification(task);
                        using (var _context = _contextFactory.CreateDbContext())
                        {
                            task.IsWorked = true;
                            await _context.SaveChangesAsync();
                        }

                        _logger.LogTrace($"Run Task:{task.Id} in Thread:{Thread.CurrentThread.ManagedThreadId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in Worker");
                        _logger.LogError(ex.ToString());

                        using (var _context = _contextFactory.CreateDbContext())
                        {
                            task.IsWorked = false;
                            await _context.SaveChangesAsync();
                        }

                        throw;
                    }
                }
            }
            catch { throw; }
            
        }

        /// <summary>
        /// Обновляет поле "IsActual" если поездка уже в прошлом
        /// </summary>
        /// <returns></returns>
        private async Task UpdateActualStatusNotificationTask()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var activeNotificationTasks = await GetActiveNotificationTasks();
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
        private async Task<IList<NotificationTask>> GetActiveNotificationTasks()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var notificationTasks = await _context.NotificationTask
                    .Where(t => t.IsActual == true)
                    .ToListAsync();
                return notificationTasks;
            }
        }

        /// <summary>
        /// Получает список задач над которыми еще не работают
        /// </summary>
        /// <returns></returns>
        private async Task<IList<NotificationTask>> GetNotWorkedNotificationTasks()
        {
            var activeNotificationTasks = await GetActiveNotificationTasks();
            var notWorkedNotificationTasks = activeNotificationTasks.Where(t => t.IsWorked == false).ToList();
            return notWorkedNotificationTasks;
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