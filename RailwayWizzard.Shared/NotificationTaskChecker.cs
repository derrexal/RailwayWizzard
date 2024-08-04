using System.Globalization;
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            var notificationTasks = await context.NotificationTask
                .Where(t => t.IsActual == true)
                .ToListAsync();
            return notificationTasks;
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
            var notWorkedNotificationTasks = await GetNotWorkedNotificationTasks();
            var result = await FillsStationCodes(notWorkedNotificationTasks);
            return result;
        }

        /// <summary>
        /// Заполняет кода городов отправления и прибытия у заданий
        /// </summary>
        /// <param name="notificationTasks">Задания для которых необходимо заполнить коды городов</param>
        /// <returns></returns>
        public async Task<IList<NotificationTask>> FillsStationCodes(IList<NotificationTask> notificationTasks)
        {
            foreach (var notificationTask in notificationTasks)
            {
                await using (var context = await _contextFactory.CreateDbContextAsync())
                {
                    var arrivalStationInfo = await context.StationInfo.SingleOrDefaultAsync(s => s.StationName == notificationTask.ArrivalStation);
                    if (arrivalStationInfo == null) throw new NullReferenceException($"Не удалось получить станцию. StationName:{notificationTask.ArrivalStation}");
                    notificationTask.ArrivalStationCode = arrivalStationInfo.ExpressCode;

                    var departureStationInfo = await context.StationInfo.SingleOrDefaultAsync(s => s.StationName == notificationTask.DepartureStation);
                    if (departureStationInfo == null) throw new NullReferenceException($"Не удалось получить станцию. StationName:{notificationTask.DepartureStation}");
                    notificationTask.DepartureStationCode = departureStationInfo.ExpressCode;
                }
            }
            return notificationTasks;
        }

        /// <summary>
        /// Выставляем задаче статус - в работе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        public async Task SetIsWorkedNotificationTask(NotificationTask inputNotificationTask)
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                if (currentNotificationTask == null) throw new NullReferenceException($"Не удалось получить задачу. ID:{inputNotificationTask.Id}");
                currentNotificationTask.IsWorked = true;
                await context.SaveChangesAsync();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Выставляет задаче статус - не актуально и не в работе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        public async Task SetIsNotActualAndIsNotWorked(NotificationTask inputNotificationTask)
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                if (currentNotificationTask == null) throw new NullReferenceException($"Не удалось получить задачу. ID:{inputNotificationTask.Id}");
                currentNotificationTask.IsActual = false;
                currentNotificationTask.IsWorked = false;
                await context.SaveChangesAsync();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Выставляет задаче статус - не актуально
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        public async Task SetIsNotWorked(NotificationTask inputNotificationTask)
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                if (currentNotificationTask == null) throw new NullReferenceException($"Не удалось получить задачу. ID:{inputNotificationTask.Id}");
                currentNotificationTask.IsWorked = false;
                await context.SaveChangesAsync();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Возвращает для задачи статус флага IsStopped
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        public async Task<bool> GetIsStoppedNotificationTask(NotificationTask inputNotificationTask)
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                if (currentNotificationTask == null) throw new NullReferenceException($"Не удалось получить задачу. ID:{inputNotificationTask.Id}");
                return currentNotificationTask.IsStopped;
            }
        }

        /// <summary>
        /// Выставляет задаче последний полученный результат
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <param name="lastResult"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task SetLastResultNotificationTask(NotificationTask inputNotificationTask, string lastResult)
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                if (currentNotificationTask == null) throw new NullReferenceException($"Не удалось получить задачу. ID:{inputNotificationTask.Id}");
                currentNotificationTask.LastResult = lastResult;
                await context.SaveChangesAsync();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Результат равен последнему?
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <param name="lastResult"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<bool> ResultIsLast(NotificationTask inputNotificationTask, string lastResult)
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                if (currentNotificationTask == null) throw new NullReferenceException($"Не удалось получить задачу. ID:{inputNotificationTask.Id}");

                if (currentNotificationTask.LastResult == lastResult) return true;
                return false;
            }
        }
    }
}