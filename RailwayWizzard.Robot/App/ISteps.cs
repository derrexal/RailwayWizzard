using RailwayWizzard.Core;


namespace RailwayWizzard.Robot.App
{
    /// <summary>
    /// Описывает бизнес-логику процесса.
    /// </summary>
    public interface ISteps
    {
        /// <summary>
        /// Здесь и происходит вся магия
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        public Task Notification(NotificationTask inputNotificationTask);
    }
}