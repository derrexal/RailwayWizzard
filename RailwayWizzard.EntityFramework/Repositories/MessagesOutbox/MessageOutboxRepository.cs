using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
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
        private readonly ILogger<MessageOutboxRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageOutboxRepository" /> class.
        /// </summary>
        /// <param name="context">Контекст БД.</param>
        /// <param name="logger">Логгер.</param>
        public MessageOutboxRepository(
            RailwayWizzardAppContext context,
            ILogger<MessageOutboxRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        //TODO: Что делать с message которые не могут отправиться из-за того что пользователь заблокировал бота
        public async Task<IEnumerable<MessageOutbox>> GetNotSendMessagesAsync()
        {
            // var hasBlockedUsers = _context.Users.Where(user => user.HasBlockedBot);
            
            var result = await _context.Messages.AsNoTracking()
                .Where(x => !x.IsSending)
                // .Where(x => !hasBlockedUsers.Any(user => user.Id == x.UserId) )
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
            var exists = await _context.Messages.AsNoTracking()
                .AnyAsync(x => x.NotificationTaskId == messageOutbox.NotificationTaskId &&
                               x.Message == messageOutbox.Message);
            if (exists)
            {
                LogDuplicate(messageOutbox, "precheck");
                return;
            }

            try
            {
                await _context.Messages.AddAsync(messageOutbox);
            
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                LogDuplicate(messageOutbox, "unique");
            }
        }
        
        private async Task<MessageOutbox> GetByIdAsync(int id)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(u => u.Id == id);
            
            if(message == null)
                throw new EntityNotFoundException($"{typeof(MessageOutbox)} with Id: {id} not found");

            return message;
        }

        private void LogDuplicate(MessageOutbox messageOutbox, string source)
        {
            var snippet = messageOutbox.Message;
            if (snippet.Length > 200)
                snippet = $"{snippet[..200]}...";

            _logger.LogInformation(
                "Duplicate AppMessageOutbox skipped (source={Source}) TaskId={TaskId} MessageLength={Length} MessageSnippet={Snippet}",
                source,
                messageOutbox.NotificationTaskId,
                messageOutbox.Message.Length,
                snippet);
        }

        private static bool IsUniqueViolation(DbUpdateException exception)
        {
            return exception.InnerException is PostgresException pg &&
                   pg.SqlState == PostgresErrorCodes.UniqueViolation;
        }
    }
}
