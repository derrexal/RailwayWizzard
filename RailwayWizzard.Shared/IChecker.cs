using RailwayWizzard.Core;

namespace RailwayWizzard.Shared
{
    /// <summary>
    /// Cодержит вспомогательные методы для сущности NotificationTask
    /// </summary>
    public interface IChecker
    {
        public bool CheckActualNotificationTask(NotificationTask task);

        public Task<IList<NotificationTask>> GetActiveNotificationTasks();

        public Task<IList<NotificationTask>> GetActiveAndNotStopNotificationTasks();

        public Task<IList<NotificationTask>> GetNotWorkedNotificationTasks();
        
        public Task<IList<NotificationTask>> GetNotificationTasksForWork();

        public Task SetIsWorkedNotificationTask(NotificationTask inputNotificationTask);

        public Task SetIsNotActualAndIsNotWorked(NotificationTask inputNotificationTask);

        public Task SetIsNotWorked(NotificationTask inputNotificationTask);

        public Task<bool> GetIsStoppedNotificationTask(NotificationTask inputNotificationTask);

        public Task SetLastResultNotificationTask(NotificationTask inputNotificationTask, string lastResult);

        public Task<bool> ResultIsLast(NotificationTask inputNotificationTask, string lastResult);
    }
}
