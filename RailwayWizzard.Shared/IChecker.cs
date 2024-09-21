using RailwayWizzard.Core;

namespace RailwayWizzard.Shared
{
    /// <summary>
    /// Cодержит вспомогательные методы для сущности NotificationTask
    /// </summary>
    public interface IChecker
    {
        /// <summary>
        /// Проверяет задачу на актуальность
        /// </summary>
        /// <param name="task">Задача</param>
        /// <returns>Результат проверки</returns>
        public bool NotificationTaskIsActual(NotificationTask task);

        /// <summary>
        /// Получает список задач со статусом "Актуально", "Не остановлена" и над которыми еще не работают
        /// </summary>
        /// <returns>Список задач</returns>
        public Task<IList<NotificationTask>> GetNotWorkedNotificationTasks();

        /// <summary>
        /// Возвращает подходящий для работы список задач
        /// </summary>
        /// <returns>Список задач</returns>
        public Task<IList<NotificationTask>> GetNotificationTasksForWork();

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
        /// Выставляет задаче последний полученный результат
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <param name="lastResult"></param>
        /// <returns>Задача</returns>
        /// <exception cref="NullReferenceException"></exception>
        public Task SetLastResultNotificationTask(NotificationTask inputNotificationTask, string lastResult);

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
