using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Common;
using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Infrastructure.Exceptions;

namespace RailwayWizzard.Infrastructure.Repositories.NotificationTasks
{
    /// <inheritdoc/>
    public class NotificationTaskRepository : INotificationTaskRepository
    {
        private readonly RailwayWizzardAppContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTaskRepository" /> class.
        /// </summary>
        /// <param name="context">Контекст БД.</param>
        public NotificationTaskRepository(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<int> CreateAsync(NotificationTask notificationTask)
        {
            await _context.AddAsync(notificationTask);

            await _context.SaveChangesAsync();

            return notificationTask.Id;
        }

        /// <inheritdoc/>
        public async Task<NotificationTask?> GetOldestAsync()
        {
            //TODO: вынести в воркер который ходит хотя бы каждые 5 минут, а не 1? или 15? Подумать...
            await UpdateActualStatusNotificationTaskAsync();

            var notWorkedNotificationTasks = GetNotWorkedNotificationTasks();

            var task = await notWorkedNotificationTasks
                .OrderBy(t => t.Updated)
                .FirstOrDefaultAsync();
            
            return task;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<NotificationTask>> GetActiveByUserAsync(long userId)
        {
            return await _context.NotificationTasks.AsNoTracking()
                .Where(u => u.IsActual)
                .Where(u => !u.IsStopped)
                .Where(u => u.CreatorId == userId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task SetIsProcessAsync(int taskId)
        {
            var currentNotificationTask = await GetNotificationTaskFromIdAsync(taskId);

            currentNotificationTask.IsProcess = true;

            await UpdateNotificationTaskAsync(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task SetIsNotWorkedAsync(int taskId)
        {
            var currentNotificationTask = await GetNotificationTaskFromIdAsync(taskId);

            currentNotificationTask.IsProcess = false;

            await UpdateNotificationTaskAsync(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task<int> SetIsStoppedAsync(int taskId)
        {
            var task = await GetNotificationTaskFromIdAsync(taskId);

            task.IsStopped = true;
            task.IsProcess = false;

            await _context.SaveChangesAsync();

            return task.Id;
        }

        /// <inheritdoc/>
        public async Task SetIsUpdatedAsync(int taskId)
        {
            var currentNotificationTask = await GetNotificationTaskFromIdAsync(taskId);

            currentNotificationTask.Updated = DateTimeExtensions.MoscowNow;

            await UpdateNotificationTaskAsync(currentNotificationTask);
        }

        public async Task<IReadOnlyCollection<int>> GetPopularStationIdsByUserIdAsync(long userId)
        {
            const int stationsLimit = 4;
            
            var popularDepartureStation = await _context.NotificationTasks
                .Where(task => task.CreatorId == userId)
                .GroupBy(task => new { UserId = task.CreatorId, City = task.DepartureStationId })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.City,
                    UsageCount = g.Count()
                })
                .ToArrayAsync();

            var popularArrivalStation = await _context.NotificationTasks
                .Where(task => task.CreatorId == userId)
                .GroupBy(task => new { UserId = task.CreatorId, City = task.ArrivalStationId })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.City,
                    UsageCount = g.Count()
                })
                .ToArrayAsync();
            
            return popularDepartureStation.Union(popularArrivalStation)
                .DistinctBy(x => x.City)
                .GroupBy(city => city.UserId)
                .SelectMany(group => group
                    .OrderByDescending(city => city.UsageCount)
                    .ThenBy(city => city.City)
                    .Take(stationsLimit))
                .Select(group => group.City)
                .ToArray();
        }
        
        /// <summary>
        /// Возвращает список актуальных, неостановленных задач, над которыми еще не производится работа.
        /// </summary>
        /// <returns>Список задач.</returns>
        private IQueryable<NotificationTask> GetNotWorkedNotificationTasks()
        {
            var notWorkedNotificationTasks = _context.NotificationTasks
                .Where(t => t.IsActual == true)
                .Where(t => t.IsStopped == false)
                .Where(t => t.IsProcess == false);

            return notWorkedNotificationTasks;
        }

        /// <summary>
        /// Возвращает сущность задачи по ее Id.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>Задача.</returns>
        /// <exception cref="EntityNotFoundException"></exception>
        private async Task<NotificationTask> GetNotificationTaskFromIdAsync(int id)
        {
            var notificationTask = await _context.NotificationTasks.FirstOrDefaultAsync(t => t.Id == id);
            if (notificationTask == null)
                throw new EntityNotFoundException($"Не удалось получить {nameof(NotificationTask)} с ID:{id}");

            return notificationTask;
        }

        /// <summary>
        /// Обновляет состояние таблицы по полю "IsActual" если поездка уже в прошлом
        /// Необходим для проверки всех активных задач, чтобы БД отражала правильное состояние
        /// </summary>
        /// <returns>Задача <see cref="Task"/>.</returns>
        private async Task UpdateActualStatusNotificationTaskAsync()
        {
            var tasks = await _context.NotificationTasks.Where(t => t.IsActual == true).ToListAsync();

            foreach (var task in tasks)
                task.IsActual = task.IsActuality();

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Обновляет задачу.
        /// </summary>
        /// <param name="notificationTask">Задача.</param>
        /// <returns>Задача <see cref="Task"/>.</returns>
        private async Task UpdateNotificationTaskAsync(NotificationTask notificationTask)
        {
            _context.NotificationTasks.Update(notificationTask);
            
            await _context.SaveChangesAsync();
        }
    }
}