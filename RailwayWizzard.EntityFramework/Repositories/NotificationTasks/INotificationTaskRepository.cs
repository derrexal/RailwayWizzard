using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Core.StationInfo;

namespace RailwayWizzard.Infrastructure.Repositories.NotificationTasks
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
        /// <returns>Наиболее старая задача.</returns>
        public Task<NotificationTask?> GetOldestAsync();

        /// <summary>
        /// Получает список активных задач по идентификатору пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список активных задач.</returns>
        public Task<IReadOnlyCollection<NotificationTask>> GetActiveByUserAsync(long userId);

        /// <summary>
        /// Выставляем задаче статус - в работе.
        /// </summary>
        /// <param name="taskId">Идентификатор задачи.</param>
        /// <returns>Задача <see cref="Task"/>.</returns>
        public Task SetIsProcessAsync(int taskId);

        /// <summary>
        /// Выставляет задаче статус - не в работе.
        /// </summary>
        /// <param name="taskId">Идентификатор задачи.</param>
        /// <returns>Задача <see cref="Task"/>.</returns>
        public Task SetIsNotWorkedAsync(int taskId);

        /// <summary>
        /// Устанавливает задаче статус "Остановлена".
        /// </summary>
        /// <param name="taskId">Идентификатор задачи.</param>
        /// <returns>Идентификатор остановленной задачи.</returns>
        public Task<int> SetIsStoppedAsync(int taskId);

        /// <summary>
        /// Устанавливает задаче поле "Updated".
        /// </summary>
        /// <param name="taskId">Идентификатор задачи.</param>
        /// <returns>Задача <see cref="Task"/>.</returns>
        public Task SetIsUpdatedAsync(int taskId);
        
        /// <summary>
        /// Возвращает список идентификаторов наиболее часто используемых пользователем городов (до 4 штук). 
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список наиболее часто используемых пользователем городов.</returns>
        public Task<IReadOnlyCollection<int>> GetPopularStationIdsByUserIdAsync(long userId);
    }
}
