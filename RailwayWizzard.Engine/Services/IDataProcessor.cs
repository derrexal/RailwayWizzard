using RailwayWizzard.Core.NotificationTask;

namespace RailwayWizzard.Engine.Services
{
    /// <summary>
    /// Описывает бизнес-логику процесса обработки задачи.
    /// </summary>
    public interface IDataProcessor
    {
        /// <summary>
        /// Запуск процесса обработки задачи.
        /// </summary>
        /// <param name="task">Задача.</param>
        /// <returns>Задача <see cref="Task"/>.</returns>
        public Task RunProcessTaskAsync(NotificationTask task);
    }
}