using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;
using RailwayWizzard.Robot.App;
using RailwayWizzard.Shared;


namespace RailwayWizzard.App
{
    public class Worker : BackgroundService
    {
        private readonly IChecker _checker;
        private readonly ILogger _logger;
        private readonly IDbContextFactory<RailwayWizzardAppContext> _contextFactory;
        public Worker(
            IChecker checker,
            ILogger<Worker> logger, 
            IDbContextFactory<RailwayWizzardAppContext> contextFactory)
        {
            _checker = checker;
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
                //Запускается 1 раз в 10 минут
                await Task.Delay(1000*60*10, cancellationToken);
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
                    await using (var context = await _contextFactory.CreateDbContextAsync(cancellationToken))
                    {
                        //TODO: Вынести наполнение в другое место?
                        //Дозаполняем кода городов
                        task.ArrivalStationCode = (await context.StationInfo.FirstOrDefaultAsync(s => s.StationName == task.ArrivalStation))!.ExpressCode;
                        task.DepartureStationCode = (await context.StationInfo.FirstOrDefaultAsync(s => s.StationName == task.DepartureStation))!.ExpressCode;
                    }
                    new StepsUsingHttpClient(_checker,_logger, _contextFactory).Notification(task);

                    _logger.LogTrace($"Run Task:{task.Id} in Thread:{Thread.CurrentThread.ManagedThreadId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Worker {ex}");
                throw; 
            }
            
        }

        /// <summary>
        /// Обновляет поле "IsActual" если поездка уже в прошлом
        /// </summary>
        /// <returns></returns>
        private async Task UpdateActualStatusNotificationTask()
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())            
            {
                var activeNotificationTasks = await GetActiveNotificationTasks();
                foreach (var activeNotificationTask in activeNotificationTasks)
                {
                    if (!_checker.CheckActualNotificationTask(activeNotificationTask))
                    {
                        activeNotificationTask.IsActual = false;
                        context.NotificationTask.Update(activeNotificationTask);
                    }
                }
                await context.SaveChangesAsync();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Получает список всех задач со статусом "Актуально"
        /// </summary>
        /// <returns></returns>
        private async Task<IList<NotificationTask>> GetActiveNotificationTasks()
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var notificationTasks = await context.NotificationTask
                    .Where(t => t.IsActual == true)
                    .ToListAsync();
                return notificationTasks;
            }
        }
        
        /// <summary>
        /// Получает список задач со статусом "Актуально" и "Не остановлена"
        /// </summary>
        /// <returns></returns>
        private async Task<IList<NotificationTask>> GetActiveAndNotStopNotificationTasks()
        {
            var activeNotificationTasks = await GetActiveNotificationTasks();
            var activeAndNotStopNotificationTasks = activeNotificationTasks.Where(t => t.IsStopped == false).ToList();
            return activeAndNotStopNotificationTasks;
        }

        /// <summary>
        /// Получает список задач над которыми еще не работают
        /// </summary>
        /// <returns></returns>
        private async Task<IList<NotificationTask>> GetNotWorkedNotificationTasks()
        {
            var activeNotificationTasks = await GetActiveAndNotStopNotificationTasks();
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