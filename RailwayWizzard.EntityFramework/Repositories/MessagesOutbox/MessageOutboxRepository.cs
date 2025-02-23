using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Common;
using RailwayWizzard.Core.MessageOutbox;
using RailwayWizzard.Infrastructure.Exceptions;

namespace RailwayWizzard.Infrastructure.Repositories.MessagesOutbox
{
    /// <summary>
    /// Репозиторий сущности <see cref="MessageOutbox"/>.
    /// </summary>
    public class MessageOutboxRepository : IMessageOutboxRepository
    {
        private readonly RailwayWizzardAppContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageOutboxRepository" /> class.
        /// </summary>
        /// <param name="context">Контекст БД.</param>
        public MessageOutboxRepository(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        //TODO: Что делать с message которые не могут отправиться из-за того что пользователь заблокировал бота
        public async Task<IEnumerable<MessageOutbox>> GetNotSendMessagesAsync()
        {
            var hasBlockedUsers = _context.Users.Where(user => user.HasBlockedBot);

            var result = await _context.Messages.AsNoTracking()
                .Where(x => !x.IsSending)
                .Where(x => !hasBlockedUsers.Any(user => user.Id == x.UserId) )
                .ToArrayAsync();
            
            return result;
        }

        public async Task SetIsSendingAsync(int id)
        {
            var message = await GetByIdAsync(id);
            
            message.IsSending = true;
            message.Send = DateTimeExtensions.MoscowNow;

            _context.Messages.Update(message);
            
            await _context.SaveChangesAsync();
        }

        public async Task CreateAsync(MessageOutbox messageOutbox)
        {
            await _context.Messages.AddAsync(messageOutbox);
            
            await _context.SaveChangesAsync();
        }
        
        private async Task<MessageOutbox> GetByIdAsync(int id)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(u => u.Id == id);
            
            if(message == null)
                throw new EntityNotFoundException($"{typeof(MessageOutbox)} with Id: {id} not found");

            return message;
        }
    }
}
