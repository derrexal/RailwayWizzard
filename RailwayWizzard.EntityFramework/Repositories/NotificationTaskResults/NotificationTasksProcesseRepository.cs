using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core.NotificationTaskResult;

namespace RailwayWizzard.Infrastructure.Repositories.NotificationTaskResults
{
    /// <inheritdoc/>
    public class NotificationTaskResultRepository : INotificationTaskResultRepository
    {
        private readonly RailwayWizzardAppContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTaskResultRepository" /> class.
        /// </summary>
        /// <param name="context">Контекст БД.</param>
        public NotificationTaskResultRepository(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        public Task<NotificationTaskResult?> GetLastNotificationTaskProcessAsync(int taskId)
        {
            return _context.NotificationTasksProcess
                .Where(x => x.NotificationTaskId == taskId)
                .Where(x => x.ResultStatus != NotificationTaskResultStatus.Error)
                .OrderByDescending(x => x.Finished)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(NotificationTaskResult notificationTaskResult)
        {
            await _context.NotificationTasksProcess.AddAsync(notificationTaskResult);
            
            await _context.SaveChangesAsync();
        }
    }
}