using RailwayWizzard.Core.TrainBotTasks;

namespace RailwayWizzard.Infrastructure.Repositories.TrainBotTasks
{
    /// <summary>
    /// Репозиторий сущности <see cref="TrainBotTaskEntity"/>.
    /// </summary>
    public class TrainBotTaskRepository : ITrainBotTaskRepository
    {
        private readonly RailwayWizzardAppContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrainBotTaskRepository" /> class.
        /// </summary>
        /// <param name="context">Контекст БД.</param>
        public TrainBotTaskRepository(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(TrainBotTaskEntity task, CancellationToken cancellationToken = default)
        {
            await _context.TrainBotTasks.AddAsync(task, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
