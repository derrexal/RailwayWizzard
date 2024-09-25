using RailwayWizzard.App.Dto.NotificationTask;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.UnitOfWork;

namespace RailwayWizzard.App.Services.NotificationTasks
{
    public class NotificationTaskService : INotificationTaskService
    {
        private readonly ILogger _logger;
        private readonly IRailwayWizzardUnitOfWork _uow;

        public NotificationTaskService(
            IRailwayWizzardUnitOfWork uow,
            ILogger<NotificationTaskService> logger)
        {
            _uow = uow;
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

            var notificationTaskId = await _uow.NotificationTaskRepository.CreateAsync(notificationTask);
            
            _logger.LogInformation($"Success create NotificationTask ID: {notificationTaskId} Details: {notificationTask.ToCustomString()}");

            return notificationTaskId;
        }

        public async Task<int?> SetIsStoppedAsync(int idNotificationTask)
        {
            return await _uow.NotificationTaskRepository.SetIsStoppedAsync(idNotificationTask);
        }

        public async Task<IReadOnlyCollection<NotificationTaskDto>> GetActiveByUserAsync(long userId)
        {
            var notificationTasks = await _uow.NotificationTaskRepository.GetActiveByUserAsync(userId);

            var result = notificationTasks.Select(u => new NotificationTaskDto
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
                .ToList();

            return result;
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}
