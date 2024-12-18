﻿using Abp.Domain.Entities;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTaskRepository" class./>
        /// </summary>
        /// <param name="context"></param>
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
        public async Task<NotificationTask?> GetOldestNotificationTask()
        {
            //TODO: вынести в воркер который ходит хотя бы каждые 5 минут, а не 1? или 15? Подумать...
            await UpdateActualStatusNotificationTask();

            var notWorkedNotificationTasks = GetNotWorkedNotificationTasks();

            var result = await notWorkedNotificationTasks
                .OrderBy(u => u.Updated)
                .FirstOrDefaultAsync();

            if (result != null && (result.DepartureStationCode==0 || result.ArrivalStationCode==0))
                result = await FillStationCodes(result);

            return result;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<NotificationTask>> GetActiveByUserAsync(long userId)
        {
            return await _context.NotificationTask.AsNoTracking()
                .Where(u => u.IsActual)
                .Where(u => !u.IsStopped)
                .Where(u => u.UserId == userId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task SetLastResultNotificationTask(NotificationTask notificationTask, string lastResult)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(notificationTask.Id);

            currentNotificationTask.LastResult = lastResult;

            await UpdateNotificationTask(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task SetIsWorkedNotificationTask(NotificationTask notificationTask)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(notificationTask.Id);

            currentNotificationTask.IsWorked = true;

            await UpdateNotificationTask(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task SetIsNotWorkedNotificationTask(NotificationTask notificationTask)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(notificationTask.Id);

            currentNotificationTask.IsWorked = false;

            await UpdateNotificationTask(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task<int?> SetIsStoppedAsync(int id)
        {
            var currentNotificationTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == id);

            if (currentNotificationTask is null)
                return null;

            currentNotificationTask.IsStopped = true;
            currentNotificationTask.IsWorked = false;

            await _context.SaveChangesAsync();

            return currentNotificationTask.Id;
        }

        /// <inheritdoc/>
        public async Task SetIsUpdatedAsync(int id)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(id);

            currentNotificationTask.Updated = Common.MoscowNow;

            await UpdateNotificationTask(currentNotificationTask);
        }

        /// <inheritdoc/>
        public async Task<NotificationTask> FillStationCodes(NotificationTask notificationTask)
        {
            // Станции на этом этапе уже должны быть в базе, так что отсутствие записи явно скажет о том что что-то сломалось
            var arrivalStationInfo = await _context.StationInfo.AsNoTracking().SingleOrDefaultAsync(s => s.StationName == notificationTask.ArrivalStation);
            if (arrivalStationInfo == null)
                throw new EntityNotFoundException($"Не удалось получить {nameof(StationInfo)} со StationName:{notificationTask.ArrivalStation}");
            notificationTask.ArrivalStationCode = arrivalStationInfo.ExpressCode;

            var departureStationInfo = await _context.StationInfo.AsNoTracking().SingleOrDefaultAsync(s => s.StationName == notificationTask.DepartureStation);
            if (departureStationInfo == null)
                throw new EntityNotFoundException($"Не удалось получить {nameof(StationInfo)} со StationName:{notificationTask.DepartureStation}");
            notificationTask.DepartureStationCode = departureStationInfo.ExpressCode;

            return notificationTask;
        }

        /// <inheritdoc/>
        public async Task<bool> ResultIsLast(NotificationTask inputNotificationTask, string lastResult)
        {
            var currentNotificationTask = await GetNotificationTaskFromId(inputNotificationTask.Id);

            return currentNotificationTask.LastResult == lastResult;
        }


        /// <summary>
        /// Возвращает список актуальных, неостановленных задач, над которыми еще не производится работа.
        /// </summary>
        /// <returns></returns>
        private IQueryable<NotificationTask> GetNotWorkedNotificationTasks()
        {
            var notWorkedNotificationTasks = _context.NotificationTask
                .Where(t => t.IsActual == true)
                .Where(t => t.IsStopped == false)
                .Where(t => t.IsWorked == false);

            return notWorkedNotificationTasks;
        }

        /// <summary>
        /// Возвращает сущность задачи по ее Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        private async Task<NotificationTask> GetNotificationTaskFromId(int id)
        {
            var notificationTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == id);
            if (notificationTask == null)
                throw new EntityNotFoundException($"Не удалось получить {nameof(NotificationTask)} с ID:{id}");

            return notificationTask;
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
                notificationTask.IsActual = notificationTask.IsActuality();

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Обновляет сущность.
        /// </summary>
        /// <param name="notificationTask"></param>
        /// <returns></returns>
        private async Task UpdateNotificationTask(NotificationTask notificationTask)
        {
            _context.NotificationTask.Update(notificationTask);
            await _context.SaveChangesAsync();
            await Task.CompletedTask;
        }
    }
}
