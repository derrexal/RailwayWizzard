using RailwayWizzard.Application.Dto.NotificationTask;
using RailwayWizzard.Common;
using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Infrastructure.Repositories.NotificationTasks;
using RailwayWizzard.Infrastructure.Repositories.StationsInfo;
using RailwayWizzard.Infrastructure.Repositories.Users;

namespace RailwayWizzard.Application.Services.NotificationTasks
{
    public class NotificationTaskService : INotificationTaskService
    {
        private readonly INotificationTaskRepository _notificationTaskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStationInfoRepository _stationInfoRepository;
        private readonly ILogger _logger;

        public NotificationTaskService(
            INotificationTaskRepository notificationTaskRepository,
            IUserRepository userRepository,
            IStationInfoRepository stationInfoRepository,
            ILogger<NotificationTaskService> logger)
        {
            _notificationTaskRepository = notificationTaskRepository;
            _userRepository = userRepository;
            _stationInfoRepository = stationInfoRepository;
            _logger = logger;
        }

        public async Task<int> CreateAsync(CreateNotificationTaskDto createNotificationTaskDto)
        {
            var arrivalStation = await _stationInfoRepository.GetByNameAsync(createNotificationTaskDto.ArrivalStation);
            var departureStation = await _stationInfoRepository.GetByNameAsync(createNotificationTaskDto.DepartureStation);
            var user = await _userRepository.GetUserByTelegramIdAsync(createNotificationTaskDto.UserId);
            
            var notificationTask = new NotificationTask
            {
                ArrivalStationId = arrivalStation.Id,
                DepartureStationId = departureStation.Id,
                DepartureDateTime = DateTime.ParseExact(createNotificationTaskDto.DateFrom.ToString("yyyy-MM-dd") + " " + createNotificationTaskDto.TimeFrom, "yyyy-MM-dd HH:mm", DateTimeExtensions.RussianCultureInfo),
                CreatorId = user.Id,
                CarTypes = createNotificationTaskDto.CarTypes,
                NumberSeats = createNotificationTaskDto.NumberSeats,
                Created = DateTimeExtensions.MoscowNow,
                Updated = DateTimeExtensions.MoscowNow,
                IsActual = true,
                IsProcess = false,
                IsStopped = false
            };
            
            var notificationTaskId = await _notificationTaskRepository.CreateAsync(notificationTask);

            _logger.LogInformation($"Success create NotificationTask ID: {notificationTaskId} Details: {notificationTask.ToLogString()}");

            return notificationTaskId;
        }

        public async Task<int?> SetIsStoppedAsync(int idNotificationTask)
        {
            return await _notificationTaskRepository.SetIsStoppedAsync(idNotificationTask);
        }

        public async Task<IReadOnlyCollection<NotificationTaskDto>> GetActiveByUserAsync(long telegramUserId)
        {
            var user = await _userRepository.GetUserByTelegramIdAsync(telegramUserId);
            var notificationTasks = await _notificationTaskRepository.GetActiveByUserAsync(user.Id);

            var result = notificationTasks.Select(async task =>
                {
                    var arrivalStation = await _stationInfoRepository.GetByIdAsync(task.ArrivalStationId);
                    var departureStation = await _stationInfoRepository.GetByIdAsync(task.DepartureStationId);
                    
                    return new NotificationTaskDto
                    {
                        Id = task.Id,
                        ArrivalStation = arrivalStation.Name,
                        DepartureStation = departureStation.Name,
                        DateFromString = task.DepartureDateTime.ToString("dd.MM.yyyy HH:mm", DateTimeExtensions.RussianCultureInfo),
                        CarTypes = string.Join(", ", task.CarTypes.Select(c => c.GetEnumDescription())),
                        NumberSeats = task.NumberSeats,
                        Updated = $"{task.Updated:t}",
                        TrainNumber = task.TrainNumber
                    };
                })
            .OrderBy(notificationTask => notificationTask.Id)
            .ToList();

            return await Task.WhenAll(result);
        }

        public async Task<IReadOnlyCollection<string>> GetPopularCitiesByUserAsync(long userId)
        {
            var topCitiesDefault = new List<string>{ "Москва", "Санкт-Петербург", "Казань", "Курск" };

            var topCitiesByUser = await _notificationTaskRepository.GetPopularStationIdsByUserIdAsync(userId);
            
            var topCitiesTasks = topCitiesByUser
                .Select(x => _stationInfoRepository.GetByIdAsync(x));

            var topCities = await Task.WhenAll(topCitiesTasks);
            
            return topCities.Length > 0 
                ? topCities.Select(x => x.Name).ToArray()
                : topCitiesDefault;
        }
    }
}
