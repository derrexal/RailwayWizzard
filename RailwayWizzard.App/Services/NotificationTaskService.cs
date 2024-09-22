using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Dto;
using RailwayWizzard.App.Services.Shared;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore;

namespace RailwayWizzard.App.Services
{
    public class NotificationTaskService : INotificationTaskService
    {
        private readonly ILogger _logger;
        private readonly RailwayWizzardAppContext _context;

        public NotificationTaskService(
            RailwayWizzardAppContext context,
            ILogger<NotificationTaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CreateAsync(CreateNotificationTaskDto createNotificationTaskDto)
        {
            //TODO: Вынести в маппинг
            var notificationTask = new NotificationTask
            {
                ArrivalStation = createNotificationTaskDto.ArrivalStation,
                DepartureStation = createNotificationTaskDto.DepartureStation,
                DateFrom = createNotificationTaskDto.DateFrom,
                TimeFrom = createNotificationTaskDto.TimeFrom,
                UserId = createNotificationTaskDto.UserId,
                CarTypes = createNotificationTaskDto.CarTypes,
                NumberSeats = createNotificationTaskDto.NumberSeats,
            };
            notificationTask.CreationTime = DateTime.Now;
            notificationTask.IsActual = true;
            notificationTask.IsWorked = false;
            notificationTask.IsStopped = false;

            //TODO: вынести в репозиторий
            _context.Add(notificationTask);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Success create NotificationTask ID: {notificationTask.Id} Details: {notificationTask.ToCustomString()}");

            return notificationTask.Id;
        }


        public async Task<int?> SetIsStoppedAsync(int idNotificationTask)
        {

            var currentTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == idNotificationTask);

            if (currentTask is null) return null;

            currentTask.IsStopped = true;
            currentTask!.IsWorked = false;

            await _context.SaveChangesAsync();
            return currentTask.Id;
        }

        public async Task<IList<NotificationTaskDto>> GetActiveByUserAsync(long userId)
        {
            var notificationTasksQuery = _context.NotificationTask
            .Where(u => u.IsActual)
            .Where(u => !u.IsStopped)
            .Where(u => u.UserId == userId)
            .AsNoTracking();

            var notificationTasks = await notificationTasksQuery.Select(u => new NotificationTaskDto
            {
                Id = u.Id,
                ArrivalStation = u.ArrivalStation,
                DepartureStation = u.DepartureStation,
                TimeFrom = u.TimeFrom,
                CarTypes = u.CarTypes,
                NumberSeats = u.NumberSeats,
                DateFromString = u.DateFromString
            })
                .OrderBy(u => u.Id)
                .ToListAsync();

            return notificationTasks;
        }
    }
}
