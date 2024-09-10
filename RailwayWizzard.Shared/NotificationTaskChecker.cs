using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;


namespace RailwayWizzard.Shared
{
    /// <inheritdoc/>
    public class NotificationTaskChecker : IChecker
    {
        private readonly IDbContextFactory<RailwayWizzardAppContext> _contextFactory;

        public NotificationTaskChecker(IDbContextFactory<RailwayWizzardAppContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <inheritdoc/>
        public bool NotificationTaskIsActual(NotificationTask task)
        {
            var moscowDateTime = Common.GetMoscowDateTime;

            DateTime notificationTaskDateTime = DateTime.ParseExact(
                        task.DateFrom.ToShortDateString() + " " + task.TimeFrom,
                        "MM/dd/yyyy HH:mm",
                        CultureInfo.InvariantCulture);

            var notificationTaskIsActual = notificationTaskDateTime > moscowDateTime;

            Console.WriteLine($"DEBUG CheckTime " +
                $"moscowDateTime:{moscowDateTime} " +
                $"notificationTaskDateTime:{notificationTaskDateTime} " +
                $"notificationTaskIsActual:{notificationTaskIsActual}");

            if (notificationTaskIsActual)
                return true;
            return false;
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationTask>> GetNotWorkedNotificationTasks()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var notWorkedNotificationTasks = await context.NotificationTask
                .Where(t => t.IsActual == true)
                .Where(t => t.IsStopped == false)
                .Where(t => t.IsWorked == false)
                .ToListAsync();

            return notWorkedNotificationTasks;
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationTask>> GetNotificationTasksForWork()
        {
            await UpdateActualStatusNotificationTask();

            var notWorkedNotificationTasks = await GetNotWorkedNotificationTasks();

            var result = await FillsStationCodes(notWorkedNotificationTasks);

            return result;
        }

        /// <inheritdoc/>
        public async Task SetIsWorkedNotificationTask(NotificationTask inputNotificationTask)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(inputNotificationTask.Id);

            currentNotificationTask.IsWorked = true;

            await UpdateNotificationTask(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task SetIsNotActualAndIsNotWorked(NotificationTask inputNotificationTask)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(inputNotificationTask.Id);

            currentNotificationTask.IsActual = false;
            currentNotificationTask.IsWorked = false;

            await UpdateNotificationTask(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task SetIsNotWorked(NotificationTask inputNotificationTask)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(inputNotificationTask.Id);

            currentNotificationTask.IsWorked = false;

            await UpdateNotificationTask(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task SetLastResultNotificationTask(NotificationTask inputNotificationTask, string lastResult)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(inputNotificationTask.Id);

            currentNotificationTask.LastResult = lastResult;

            await UpdateNotificationTask(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task<bool> ResultIsLast(NotificationTask inputNotificationTask, string lastResult)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(inputNotificationTask.Id);
            if (currentNotificationTask.LastResult == lastResult) return true;
            return false;
        }

        /// <summary>
        /// Заполняет кода городов отправления и прибытия у заданий
        /// </summary>
        /// <param name="notificationTasks">Задания для которых необходимо заполнить коды городов</param>
        /// <returns></returns>
        private async Task<IList<NotificationTask>> FillsStationCodes(IList<NotificationTask> notificationTasks)
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
        /// Обновляет состояние таблицы по полю "IsActual" если поездка уже в прошлом
        /// Это необходимо для проверки всех активных задач, чтобы БД отражала правильное состояние
        /// </summary>
        /// <returns></returns>
        private async Task UpdateActualStatusNotificationTask()
        {
            await using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var notificationTasks = await context.NotificationTask.Where(t => t.IsActual == true).ToListAsync();

                foreach (var notificationTask in notificationTasks)
                    if (!NotificationTaskIsActual(notificationTask))
                    {
                        notificationTask.IsActual = false;
                        context.NotificationTask.Update(notificationTask);
                    }
                await context.SaveChangesAsync();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Возвращает сущность задачи по ее Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        private async Task<NotificationTask> GetNotificationTaskFromId(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var currentNotificationTask = await context.NotificationTask.FirstAsync(t => t.Id == id);
            if (currentNotificationTask == null) throw new NullReferenceException($"Не удалось получить задачу. ID:{id}");

            return currentNotificationTask;
        }

        /// <summary>
        /// Обновляет состояние задачи
        /// </summary>
        /// <param name="notificationTask"></param>
        /// <returns></returns>
        private async Task UpdateNotificationTask(NotificationTask notificationTask)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.NotificationTask.Update(notificationTask);
            await context.SaveChangesAsync();
            await Task.CompletedTask;
        }
    }
}