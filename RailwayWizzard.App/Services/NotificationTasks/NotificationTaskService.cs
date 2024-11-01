using RailwayWizzard.App.Dto.NotificationTask;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App.Services.NotificationTasks
{
    public class NotificationTaskService : INotificationTaskService
    {
        private readonly INotificationTaskRepository _notificationTaskRepository;
        private readonly ILogger _logger;

        public NotificationTaskService(
            INotificationTaskRepository notificationTaskRepository,
            ILogger<NotificationTaskService> logger)
        {
            _notificationTaskRepository = notificationTaskRepository;
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
            notificationTask.CreationTime = Common.MoscowNow;
            notificationTask.IsActual = true;
            notificationTask.IsWorked = false;
            notificationTask.IsStopped = false;
            notificationTask.Updated = Common.MoscowNow;

            var notificationTaskId = await _notificationTaskRepository.CreateAsync(notificationTask);

            _logger.LogInformation($"Success create NotificationTask ID: {notificationTaskId} Details: {notificationTask.ToCustomString()}");

            return notificationTaskId;
        }

        public async Task<int?> SetIsStoppedAsync(int idNotificationTask)
        {
            return await _notificationTaskRepository.SetIsStoppedAsync(idNotificationTask);
        }

        public async Task<IReadOnlyCollection<NotificationTaskDto>> GetActiveByUserAsync(long userId)
        {
            var notificationTasks = await _notificationTaskRepository.GetActiveByUserAsync(userId);

            var result = notificationTasks.Select(u => new NotificationTaskDto
            {
                Id = u.Id,
                ArrivalStation = u.ArrivalStation,
                DepartureStation = u.DepartureStation,
                TimeFrom = u.TimeFrom,
                CarTypes = u.CarTypes,
                NumberSeats = u.NumberSeats,
                DateFromString = u.DateFromString,
                TimeUpdated = $"{u.Updated:t}" ?? ""
            })
                .OrderBy(u => u.Id)
                .ToList();

            return result;
        }
    }
}
