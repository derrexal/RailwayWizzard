using RailwayWizzard.App.Dto.NotificationTask;

namespace RailwayWizzard.App.Services.NotificationTasks
{
    public interface INotificationTaskService
    {
        public Task<int> CreateAsync(CreateNotificationTaskDto notificationTask);
        public Task<int?> SetIsStoppedAsync(int idNotificationTask);
        public Task<IReadOnlyCollection<NotificationTaskDto>> GetActiveByUserAsync(long userId);
    }
}
