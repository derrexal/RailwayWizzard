using RailwayWizzard.Core;

namespace RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks
{
    /// <summary>
    /// Репозиторий сущности <see cref="StationInfo"/>.
    /// </summary>
    public interface INotificationTaskRepository
    {
        /// <summary>
        /// Инициализирует начальное состояние базы.
        /// </summary>
        /// <returns></returns>
        public Task DatabaseInitialize();

        /// <summary>
        /// Возвращает сущность задачи по ее Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        public Task<NotificationTask> GetNotificationTaskFromId(int id);

        /// <summary>
        /// Получает список задач со статусом "Актуально", "Не остановлена" и над которыми еще не работают
        /// </summary>
        /// <returns>Список задач</returns>
        public Task<IList<NotificationTask>> GetNotWorkedNotificationTasks();

        /// <summary>
        /// Возвращает результат сравнения последнего результата с текущим
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <param name="lastResult"></param>
        /// <returns>Задача</returns>
        /// <exception cref="NullReferenceException"></exception>
        public Task<bool> ResultIsLast(NotificationTask inputNotificationTask, string lastResult);

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
        /// Выставляет задаче статус - не актуально и не в работе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns>Задача</returns
        public Task SetIsNotActualAndIsNotWorked(NotificationTask inputNotificationTask);

        /// <summary>
        /// Выставляет задаче статус - не в работе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns>Задача</returns>
        public Task SetIsNotWorked(NotificationTask inputNotificationTask);

        /// <summary>
        /// Возвращает подходящий для работы список задач
        /// </summary>
        /// <returns>Список задач</returns>
        public Task<IList<NotificationTask>> GetNotificationTasksForWork();

        /// <summary>
        /// Добавляет сущность <see cref="NotificationTask"/> в БД.
        /// </summary>
        /// <param name="notificationTask">Задача для добавления.</param>
        /// <returns>Идентификатор добавленной сущности.</returns>
        public Task<int> CreateAsync(NotificationTask notificationTask);

        /// <summary>
        /// Устанавливает задаче статус "Остановлена".
        /// </summary>
        /// <param name="idNotificationTask"></param>
        /// <returns>Идентификатор остановленной задачи.</returns>
        public Task<int?> SetIsStoppedAsync(int idNotificationTask);

        /// <summary>
        /// Получает список активных задач по идентификатору пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список активных задач.</returns>
        public Task<IReadOnlyCollection<NotificationTask>> GetActiveByUserAsync(long userId);
    }
}
