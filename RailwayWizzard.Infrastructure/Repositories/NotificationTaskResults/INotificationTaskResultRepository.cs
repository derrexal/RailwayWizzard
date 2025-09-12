using RailwayWizzard.Core.StationInfo;

namespace RailwayWizzard.Infrastructure.Repositories.NotificationTaskResults
{
    /// <summary>
    /// Репозиторий сущности <see cref="StationInfo"/>.
    /// </summary>
    public interface INotificationTaskResultRepository
    {
        /// <summary>
        /// Получить последнюю запись о выполнении указанной задачи.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public Task<Core.NotificationTaskResult.NotificationTaskResult?> GetLastNotificationTaskProcessAsync(int taskId);

        /// <summary>
        /// Создает запись о выполнении задачи.
        /// </summary>
        /// <param name="notificationTaskResult">Модель выполненной задаче.</param>
        /// <returns></returns>
        public Task CreateAsync(Core.NotificationTaskResult.NotificationTaskResult notificationTaskResult);
    }
}
