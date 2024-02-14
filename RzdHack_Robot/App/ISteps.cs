using RzdHack.Robot.Core;

namespace RzdHack.Robot.App
{
    public interface ISteps
    {
        public Task Notification(NotificationTask message);
    }
}
