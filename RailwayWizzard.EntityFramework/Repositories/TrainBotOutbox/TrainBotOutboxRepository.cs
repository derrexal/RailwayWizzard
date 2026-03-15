using RailwayWizzard.Core.TrainBotOutbox;

namespace RailwayWizzard.Infrastructure.Repositories.TrainBotOutbox
{
    /// <summary>
    /// Репозиторий сущности <see cref="TrainBotMessage"/>.
    /// </summary>
    public class TrainBotOutboxRepository : ITrainBotOutboxRepository
    {
        private readonly RailwayWizzardAppContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrainBotOutboxRepository" /> class.
        /// </summary>
        /// <param name="context">Контекст БД.</param>
        public TrainBotOutboxRepository(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        public async Task<TrainBotMessage> CreateAsync(TrainBotMessage message)
        {
            await _context.TrainBotOutboxMessages.AddAsync(message);
            await _context.SaveChangesAsync();

            return message;
        }
    }
}
