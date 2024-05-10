using System.Globalization;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;


namespace RailwayWizzard.Shared
{
    /// <summary>
    /// Cодержит вспомогательные методы для сущности NotificationTask
    /// </summary>
    public class NotificationTaskChecker : IChecker
    {
        private readonly IDbContextFactory<RailwayWizzardAppContext> _contextFactory;

        public NotificationTaskChecker(IDbContextFactory<RailwayWizzardAppContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Проверяет задачу на актуальность
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool CheckActualNotificationTask(NotificationTask task)
        {
            DateTime itemDateFromDateTime = DateTime.ParseExact(
                        task.DateFrom.ToShortDateString() + " " + task.TimeFrom,
                        "MM/dd/yyyy HH:mm",
                        CultureInfo.InvariantCulture);

            if (itemDateFromDateTime < DateTime.Now)
                return false;
            return true;
        }

        /// <summary>
        /// Обновляет состояние таблицы по полю "IsActual" если поездка уже в прошлом
        /// </summary>
        /// <returns></returns>
        private async Task UpdateActualStatusNotificationTask()
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var activeNotificationTasks = await GetActiveNotificationTasks();
                foreach (var activeNotificationTask in activeNotificationTasks)
                {
                    if (!CheckActualNotificationTask(activeNotificationTask))
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
        public async Task<IList<NotificationTask>> GetActiveNotificationTasks()
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
        public async Task<IList<NotificationTask>> GetActiveAndNotStopNotificationTasks()
        {
            await UpdateActualStatusNotificationTask();
            var activeNotificationTasks = await GetActiveNotificationTasks();
            var activeAndNotStopNotificationTasks = activeNotificationTasks.Where(t => t.IsStopped == false).ToList();
            return activeAndNotStopNotificationTasks;
        }

        /// <summary>
        /// Получает список задач со статусом "Актуально", "Не остановлена" и над которыми еще не работают
        /// </summary>
        /// <returns></returns>
        public async Task<IList<NotificationTask>> GetNotWorkedNotificationTasks()
        {
            await UpdateActualStatusNotificationTask();
            var activeNotificationTasks = await GetActiveAndNotStopNotificationTasks();
            var notWorkedNotificationTasks = activeNotificationTasks.Where(t => t.IsWorked == false).ToList();
            return notWorkedNotificationTasks;
        }

        /// <summary>
        /// Возвращает подходящий для работы список задач
        /// </summary>
        /// <returns></returns>
        public async Task<IList<NotificationTask>> GetNotificationTasksForWork()
        {
            try
            {
                var notWorkedNotificationTasks = await GetNotWorkedNotificationTasks();
                var result = await FillsStationCodes(notWorkedNotificationTasks);
                return result;
            }
            catch { throw; }
        }

        /// <summary>
        /// Заполняет кода городов отправления и прибытия у сущности 
        /// </summary>
        /// <param name="notificationTasks"></param>
        /// <returns></returns>
        public async Task<IList<NotificationTask>> FillsStationCodes(IList<NotificationTask> notificationTasks)
        {
            try
            {
                foreach (var task in notificationTasks)
                {
                    await using (var context = await _contextFactory.CreateDbContextAsync())
                    {
                        task.ArrivalStationCode = (await context.StationInfo.FirstOrDefaultAsync(s => s.StationName == task.ArrivalStation))!.ExpressCode;
                        task.DepartureStationCode = (await context.StationInfo.FirstOrDefaultAsync(s => s.StationName == task.DepartureStation))!.ExpressCode;
                    }
                }
                return notificationTasks;
            }
            catch { throw; }
        }
    }
}