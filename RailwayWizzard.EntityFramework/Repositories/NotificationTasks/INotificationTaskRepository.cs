using RailwayWizzard.Core;

namespace RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks
{
    /// <summary>
    /// Репозиторий сущности <see cref="StationInfo"/>.
    /// </summary>
    public interface INotificationTaskRepository
    {
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
        /// Обновляет состояние задачи
        /// </summary>
        /// <param name="notificationTask"></param>
        /// <returns></returns>
        public Task UpdateNotificationTask(NotificationTask notificationTask);

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
        /// Проверяет задачу на актуальность
        /// </summary>
        /// <param name="task">Задача</param>
        /// <returns>Результат проверки</returns>
        public bool NotificationTaskIsActual(NotificationTask task);

        /// <summary>
        /// Возвращает подходящий для работы список задач
        /// </summary>
        /// <returns>Список задач</returns>
        public Task<IList<NotificationTask>> GetNotificationTasksForWork();

        public Task<int> CreateAsync(NotificationTask notificationTask);

        public Task<int?> SetIsStoppedAsync(int idNotificationTask);

        public Task<IReadOnlyCollection<NotificationTask>> GetActiveByUserAsync(long userId);
    }
}
