using System.Globalization;
using Abp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RailwayWizzard.Shared;

namespace RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks
{
    /// <inheritdoc/>
    public class NotificationTaskRepository : INotificationTaskRepository
    {
        private readonly RailwayWizzardAppContext _context;

        public NotificationTaskRepository(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<NotificationTask> GetNotificationTaskFromId(int id)
        {
            var currentNotificationTask = await _context.NotificationTask.FirstAsync(t => t.Id == id);
            if (currentNotificationTask == null) 
                throw new EntityNotFoundException($"Не удалось получить задачу. ID:{id}");

            return currentNotificationTask;
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationTask>> GetNotWorkedNotificationTasks()
        {
            var notWorkedNotificationTasks = await _context.NotificationTask
                .Where(t => t.IsActual == true)
                .Where(t => t.IsStopped == false)
                .Where(t => t.IsWorked == false)
                .ToListAsync();

            return notWorkedNotificationTasks;
        }

        /// <inheritdoc/>
        public async Task UpdateNotificationTask(NotificationTask notificationTask)
        {
            _context.NotificationTask.Update(notificationTask);
            await _context.SaveChangesAsync();
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<bool> ResultIsLast(NotificationTask inputNotificationTask, string lastResult)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(inputNotificationTask.Id);
            if (currentNotificationTask.LastResult == lastResult) return true;
            return false;
        }

        /// <inheritdoc/>
        public async Task SetLastResultNotificationTask(NotificationTask inputNotificationTask, string lastResult)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(inputNotificationTask.Id);

            currentNotificationTask.LastResult = lastResult;

            await UpdateNotificationTask(currentNotificationTask);
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
        // TODO: вынести отсюда. Никакого отношения к сущности и базе не имеет...
        public bool NotificationTaskIsActual(NotificationTask task)
        {
            DateTime notificationTaskDateTime = DateTime.ParseExact(
            task.DateFrom.ToShortDateString() + " " + task.TimeFrom,
            "MM/dd/yyyy HH:mm",
            CultureInfo.InvariantCulture);

            var moscowDateTime = Common.GetMoscowDateTime;

            var notificationTaskIsActual = notificationTaskDateTime > moscowDateTime;
            if (notificationTaskIsActual)
                return true;
            return false;
        }


        /// <inheritdoc/>
        public async Task<IList<NotificationTask>> GetNotificationTasksForWork()
        {
            await UpdateActualStatusNotificationTask();

            var notWorkedNotificationTasks = await GetNotWorkedNotificationTasks();

            var result = await FillsStationCodes(notWorkedNotificationTasks);

            return result;
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
                var arrivalStationInfo = await _context.StationInfo.SingleOrDefaultAsync(s => s.StationName == notificationTask.ArrivalStation);
                if (arrivalStationInfo == null) 
                    throw new EntityNotFoundException($"Не удалось получить станцию. StationName:{notificationTask.ArrivalStation}");
                notificationTask.ArrivalStationCode = arrivalStationInfo.ExpressCode;

                var departureStationInfo = await _context.StationInfo.SingleOrDefaultAsync(s => s.StationName == notificationTask.DepartureStation);
                if (departureStationInfo == null) 
                    throw new EntityNotFoundException($"Не удалось получить станцию. StationName:{notificationTask.DepartureStation}");
                notificationTask.DepartureStationCode = departureStationInfo.ExpressCode;
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
            {
                var notificationTasks = await _context.NotificationTask.Where(t => t.IsActual == true).ToListAsync();

                foreach (var notificationTask in notificationTasks)
                    if (!NotificationTaskIsActual(notificationTask))
                    {
                        notificationTask.IsActual = false;
                        _context.NotificationTask.Update(notificationTask);
                    }
                await _context.SaveChangesAsync();
            }
            await Task.CompletedTask;
        }

        public async Task<int> CreateAsync(NotificationTask notificationTask)
        {
            _context.Add(notificationTask);

            await _context.SaveChangesAsync();
            
            return notificationTask.Id;
        }

        public async Task<int?> SetIsStoppedAsync(int idNotificationTask)
        {
            var currentTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == idNotificationTask);

            if (currentTask is null) return null;

            currentTask.IsStopped = true;
            currentTask!.IsWorked = false;

            await _context.SaveChangesAsync();
            
            return currentTask.Id;
        }

        public async Task<IReadOnlyCollection<NotificationTask>> GetActiveByUserAsync(long userId)
        {
            return await _context.NotificationTask
                .Where(u => u.IsActual)
                .Where(u => !u.IsStopped)
                .Where(u => u.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
