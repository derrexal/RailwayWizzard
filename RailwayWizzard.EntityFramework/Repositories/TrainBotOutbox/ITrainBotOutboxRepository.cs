using RailwayWizzard.Core.TrainBotOutbox;

namespace RailwayWizzard.Infrastructure.Repositories.TrainBotOutbox
{
    /// <summary>
    /// Репозиторий сущности <see cref="TrainBotMessage"/>.
    /// </summary>
    public interface ITrainBotOutboxRepository
    {
        /// <summary>
        /// Создает запись о полученном сообщении.
        /// </summary>
        /// <param name="message">Сообщение из RabbitMQ.</param>
        /// <returns>Созданная запись.</returns>
        Task<TrainBotMessage> CreateAsync(TrainBotMessage message);
    }
}
