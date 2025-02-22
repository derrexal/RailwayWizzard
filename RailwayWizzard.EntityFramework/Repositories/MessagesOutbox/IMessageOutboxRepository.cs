using RailwayWizzard.Core.MessageOutbox;

namespace RailwayWizzard.Infrastructure.Repositories.MessagesOutbox
{
    /// <summary>
    /// Репозиторий сущности <see cref="MessageOutbox"/>.
    /// </summary>
    public interface IMessageOutboxRepository
    {
        /// <summary>
        /// Получить список <see cref="MessageOutbox" /> которые еще не отправили пользователям.
        /// </summary>
        /// <returns>Список <see cref="MessageOutbox" />.</returns>
        public Task<IEnumerable<MessageOutbox>> GetNotSendMessagesAsync();
        
        /// <summary>
        /// Устанавливает флаг об успешном отправлении сообщения.
        /// </summary>
        /// <returns>Задача <see cref="Task"/>.</returns>
        public Task SetIsSendingAsync(int id);
        
        /// <summary>
        /// Создает запись об отправление сообщении пользователю.
        /// </summary>
        /// <param name="messageOutbox">Сообщение которое необходимо отправить пользователю.</param>
        /// <returns>Задача <see cref="Task"/>.</returns>
        public Task CreateAsync(MessageOutbox messageOutbox);
    }
}
