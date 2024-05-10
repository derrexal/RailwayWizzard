using RailwayWizzard.Core;

namespace RailwayWizzard.Shared
{
    /// <summary>
    /// Cодержит вспомогательные методы для сущности NotificationTask
    /// </summary>
    public interface IChecker
    {
        public bool CheckActualNotificationTask(NotificationTask task);
    }
}
