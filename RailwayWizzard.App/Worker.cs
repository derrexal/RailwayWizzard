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
                //TODO: ���-���� �������, ����� ������ �� ������ �������� ��������������� ����� ��������
                //����������� 1 ��� � 15 �����
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
                    using (var _context = _contextFactory.CreateDbContext())
                    {
                        //TODO: ������� ���������� � ������ �����?
                        //����������� ���� �������
                        task.ArrivalStationCode = (await _context.StationInfo.FirstOrDefaultAsync(s => s.StationName == task.ArrivalStation))!.ExpressCode;
                        task.DepartureStationCode = (await _context.StationInfo.FirstOrDefaultAsync(s => s.StationName == task.DepartureStation))!.ExpressCode;
                    }
                    new StepsUsingHttpClient(_checker,_logger, _contextFactory).Notification(task);

                    _logger.LogTrace($"Run Task:{task.Id} in Thread:{Thread.CurrentThread.ManagedThreadId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Worker");
                _logger.LogError(ex.ToString());
                throw; 
            }
            
        }

        /// <summary>
        /// ��������� ���� "IsActual" ���� ������� ��� � �������
        /// </summary>
        /// <returns></returns>
        private async Task UpdateActualStatusNotificationTask()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var activeNotificationTasks = await GetActiveNotificationTasks();
                foreach (var activeNotificationTask in activeNotificationTasks)
                {
                    if (!_checker.CheckActualNotificationTask(activeNotificationTask))
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
        /// �������� ������ ����� �� �������� "���������" � "�� �����������"
        /// </summary>
        /// <returns></returns>
        private async Task<IList<NotificationTask>> GetActiveNotificationTasks()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var notificationTasks = await _context.NotificationTask
                    .Where(t => t.IsActual == true)
                    .Where(t => t.IsStopped == false)
                    .ToListAsync();
                return notificationTasks;
            }
        }

        /// <summary>
        /// �������� ������ ����� ��� �������� ��� �� ��������
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