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
        /// <param name="task"></param>
        /// <returns></returns>
        public bool NotificationTaskIsActual(NotificationTask task);

        /// <summary>
        /// Получает список задач со статусом "Актуально", "Не остановлена" и над которыми еще не работают
        /// </summary>
        /// <returns></returns>
        public Task<IList<NotificationTask>> GetNotWorkedNotificationTasks();

        /// <summary>
        /// Возвращает подходящий для работы список задач
        /// </summary>
        /// <returns></returns>
        public Task<IList<NotificationTask>> GetNotificationTasksForWork();

        /// <summary>
        /// Выставляем задаче статус - в работе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        public Task SetIsWorkedNotificationTask(NotificationTask inputNotificationTask);

        /// <summary>
        /// Выставляет задаче статус - не актуально и не в работе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns
        public Task SetIsNotActualAndIsNotWorked(NotificationTask inputNotificationTask);
        
        /// <summary>
        /// Выставляет задаче статус - не в работе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        public Task SetIsNotWorked(NotificationTask inputNotificationTask);

        /// <summary>
        /// Выставляет задаче последний полученный результат
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <param name="lastResult"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public Task SetLastResultNotificationTask(NotificationTask inputNotificationTask, string lastResult);

        /// <summary>
        /// Возвращает результат сравнения последнего результата с текущим
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <param name="lastResult"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public Task<bool> ResultIsLast(NotificationTask inputNotificationTask, string lastResult);
    }
}
