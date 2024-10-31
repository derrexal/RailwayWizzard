using Abp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RailwayWizzard.Core.Shared;
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

        public async Task DatabaseInitialize()
        {
            if (await _context.Database.CanConnectAsync() == false) 
                await Task.Delay(5000);

            // Applying migrations to run program
            await _context.Database.MigrateAsync();

            // Before Run Program Update field IsWorked default value(false)
            _context.NotificationTask.ExecuteUpdate(t =>
                t.SetProperty(t => t.IsWorked, false));
        }

        /// <inheritdoc/>
        public async Task<NotificationTask> GetNotificationTaskFromId(int id)
        {
            var notificationTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == id);
            if (notificationTask == null)    
                throw new EntityNotFoundException($"Не удалось получить задачу. ID:{id}");

            return notificationTask;
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
            if (currentNotificationTask.LastResult == lastResult) 
                return true;
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
                var arrivalStationInfo = await _context.StationInfo.AsNoTracking().SingleOrDefaultAsync(s => s.StationName == notificationTask.ArrivalStation);
                if (arrivalStationInfo == null) 
                    throw new EntityNotFoundException($"Не удалось получить станцию. StationName:{notificationTask.ArrivalStation}");
                notificationTask.ArrivalStationCode = arrivalStationInfo.ExpressCode;

                var departureStationInfo = await _context.StationInfo.AsNoTracking().SingleOrDefaultAsync(s => s.StationName == notificationTask.DepartureStation);
                if (departureStationInfo == null) 
                    throw new EntityNotFoundException($"Не удалось получить станцию. StationName:{notificationTask.DepartureStation}");
                notificationTask.DepartureStationCode = departureStationInfo.ExpressCode;
            }
            return notificationTasks;
        }

        /// <summary>
        /// Обновляет состояние таблицы по полю "IsActual" если поездка уже в прошлом
        /// Необходим для проверки всех активных задач, чтобы БД отражала правильное состояние
        /// </summary>
        /// <returns></returns>
        private async Task UpdateActualStatusNotificationTask()
        {
            var notificationTasks = await _context.NotificationTask.Where(t => t.IsActual == true).ToListAsync();

            foreach (var notificationTask in notificationTasks)
                if (notificationTask.IsActuality() == false)
                    notificationTask.IsActual = false;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(NotificationTask notificationTask)
        {
            await _context.AddAsync(notificationTask);

            await _context.SaveChangesAsync();
            
            return notificationTask.Id;
        }

        public async Task<int?> SetIsStoppedAsync(int idNotificationTask)
        {
            var currentNotificationTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == idNotificationTask);

            if (currentNotificationTask is null) 
                return null;

            currentNotificationTask.IsStopped = true;
            currentNotificationTask.IsWorked = false;

            await _context.SaveChangesAsync();
            
            return currentNotificationTask.Id;
        }

        public async Task<IReadOnlyCollection<NotificationTask>> GetActiveByUserAsync(long userId)
        {
            return await _context.NotificationTask.AsNoTracking()
                .Where(u => u.IsActual)
                .Where(u => !u.IsStopped)
                .Where(u => u.UserId == userId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task SetIsUpdatedAsync(int idNotificationTask)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(idNotificationTask);

            currentNotificationTask.Updated = Common.MoscowNow;

            await UpdateNotificationTask(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task<NotificationTask?> GetOldestNotificationTask()
        {
            const int totalTime = 1000 * 30; // Чтобы задача опрашивалась не чаще чем 1 раз в 30 секунд

            var today = Common.MoscowNow.AddMilliseconds(-totalTime);

            // TODO: Если создать миграцию которая проставляет всем существующим таскам поле Updated - этот костыль будет не нужен
            var result = await _context.NotificationTask
                .Where(t => t.IsActual)
                .Where(t => t.IsStopped == false)
                .Where(t => t.Updated == null)
                .FirstOrDefaultAsync();

            if (result == null)
                result = await _context.NotificationTask
                    .Where(t => t.IsActual)
                    .Where(t => t.IsStopped == false)
                    .Where(t => t.Updated != null)
                    .Where(t => t.Updated < today)
                    .OrderBy(u => u.Updated)
                    .FirstOrDefaultAsync();

            return result;
        }
    }
}
