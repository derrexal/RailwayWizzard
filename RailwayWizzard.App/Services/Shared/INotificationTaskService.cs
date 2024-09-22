using RailwayWizzard.App.Dto;
using RailwayWizzard.Core;

namespace RailwayWizzard.App.Services.Shared
{
    public interface INotificationTaskService
    {
        public Task<int> CreateAsync(CreateNotificationTaskDto notificationTask);
        public Task<int?> SetIsStoppedAsync(int idNotificationTask);
        public Task<IList<NotificationTaskDto>> GetActiveByUserAsync(long userId);
    }
}
