﻿using RailwayWizzard.Core;

namespace RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks
{
    /// <summary>
    /// Репозиторий сущности <see cref="StationInfo"/>.
    /// </summary>
    public interface INotificationTaskRepository
    {
        /// <summary>
        /// Добавляет сущность <see cref="NotificationTask"/> в БД.
        /// </summary>
        /// <param name="notificationTask">Задача для добавления.</param>
        /// <returns>Идентификатор добавленной сущности.</returns>
        public Task<int> CreateAsync(NotificationTask notificationTask);

        //TODO: потенциально - кучу лишних запросов к БД... Нужно переделать на очередь, т.к. состояние задач в плане их "устаревания" не сильно меняется (кажется)
        /// <summary>
        /// Возвращает задачу которую дольше всего не обрабатывали.
        /// </summary>
        /// <returns>Список задач</returns>
        public Task<NotificationTask?> GetOldestNotificationTask();

        /// <summary>
        /// Получает список активных задач по идентификатору пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список активных задач.</returns>
        public Task<IReadOnlyCollection<NotificationTask>> GetActiveByUserAsync(long userId);

        /// <summary>
        /// Выставляет задаче последний полученный результат
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <param name="lastResult"></param>
        /// <returns>Задача</returns>
        /// <exception cref="NullReferenceException"></exception>
        public Task SetLastResultNotificationTask(NotificationTask inputNotificationTask, string lastResult);

        /// <summary>
        /// Выставляем задаче статус - в работе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns>Задача</returns>
        public Task SetIsWorkedNotificationTask(NotificationTask inputNotificationTask);

        /// <summary>
        /// Выставляет задаче статус - не в работе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns>Задача</returns>
        public Task SetIsNotWorkedNotificationTask(NotificationTask inputNotificationTask);

        /// <summary>
        /// Устанавливает задаче статус "Остановлена".
        /// </summary>
        /// <param name="idNotificationTask"></param>
        /// <returns>Идентификатор остановленной задачи.</returns>
        public Task<int?> SetIsStoppedAsync(int idNotificationTask);

        /// <summary>
        /// Устанавливает задаче поле "Updated".
        /// </summary>
        /// <param name="idNotificationTask"></param>
        /// <returns>Идентификатор задачи.</returns>
        public Task SetIsUpdatedAsync(int idNotificationTask);

        /// <summary>
        /// Заполняет код города отправления и прибытия у задания
        /// </summary>
        /// <param name="notificationTasks">Задание для которого необходимо заполнить коды городов</param>
        /// <returns></returns>
        public Task<NotificationTask> FillStationCodes(NotificationTask notificationTask);


        /// <summary>
        /// Возвращает результат сравнения последнего результата с текущим
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <param name="lastResult"></param>
        /// <returns>Задача</returns>
        /// <exception cref="NullReferenceException"></exception>
        public Task<bool> ResultIsLast(NotificationTask inputNotificationTask, string lastResult);
    }
}
