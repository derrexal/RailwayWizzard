using RailwayWizzard.Core.TrainBotTasks;

namespace RailwayWizzard.Infrastructure.Repositories.TrainBotTasks
{
    /// <summary>
    /// Репозиторий сущности <see cref="TrainBotTaskEntity"/>.
    /// </summary>
    public interface ITrainBotTaskRepository
    {
        /// <summary>
        /// Создает запись задачи из train_bot.
        /// </summary>
        /// <param name="task">Задача.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task CreateAsync(TrainBotTaskEntity task, CancellationToken cancellationToken = default);
    }
}
