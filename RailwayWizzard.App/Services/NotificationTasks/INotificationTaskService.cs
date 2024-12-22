using RailwayWizzard.App.Dto.NotificationTask;

namespace RailwayWizzard.App.Services.NotificationTasks
{
    /// <summary>
    ///  Сервис задач по уведомлениям.
    /// </summary>
    public interface INotificationTaskService
    {
        /// <summary>
        /// Создать задачу.
        /// </summary>
        /// <param name="notificationTask">Параметры создаваемой задачи.</param>
        /// <returns>Идентификатор созданной задачи.</returns>
        public Task<int> CreateAsync(CreateNotificationTaskDto notificationTask);

        /// <summary>
        /// Переводит задачу в статус "Остановлена пользователем".
        /// </summary>
        /// <param name="idNotificationTask">Идентификатор останавливаемой задачи.</param>
        /// <returns>Идентификатор остановленной задачи.</returns>
        public Task<int?> SetIsStoppedAsync(int idNotificationTask);

        /// <summary>
        /// Получить список активных задач пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Коллекция задач.</returns>
        public Task<IReadOnlyCollection<NotificationTaskDto>> GetActiveByUserAsync(long userId);
    }
}
