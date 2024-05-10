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
    }
}
