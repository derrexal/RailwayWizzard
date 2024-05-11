using RailwayWizzard.Core;
namespace RailwayWizzard.Robot.App
{
    /// <summary>
    /// Получаем информацию от сервисов РЖД
    /// </summary>
    public interface IRobot
    {
        public Task<List<string>> GetFreeSeatsOnTheTrain(NotificationTask inputNotificationTask);
    }
}
