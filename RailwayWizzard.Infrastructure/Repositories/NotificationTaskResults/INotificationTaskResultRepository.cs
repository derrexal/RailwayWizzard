using RailwayWizzard.Core.NotificationTaskResult;

namespace RailwayWizzard.Infrastructure.Repositories.NotificationTaskResults
{
    /// <summary>
    /// Репозиторий сущности <see cref="NotificationTaskResult"/>.
    /// </summary>
    public interface INotificationTaskResultRepository
    {
        /// <summary>
        /// Получить последнюю запись о выполнении указанной задачи.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public Task<NotificationTaskResult?> GetLastNotificationTaskProcessAsync(int taskId);

        /// <summary>
        /// Создает запись о выполнении задачи.
        /// </summary>
        /// <param name="notificationTaskResult">Модель выполненной задаче.</param>
        /// <returns></returns>
        public Task CreateAsync(NotificationTaskResult notificationTaskResult);
    }
}
