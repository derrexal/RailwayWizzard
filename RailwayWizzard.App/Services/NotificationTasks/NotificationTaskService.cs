﻿using RailwayWizzard.App.Dto.NotificationTask;
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
            
            // TODO: Вынести в маппинг

            var notificationTask = new NotificationTask
            {
                ArrivalStation = createNotificationTaskDto.ArrivalStation,
                DepartureStation = createNotificationTaskDto.DepartureStation,
                DepartureDateTime = DateTime.ParseExact(createNotificationTaskDto.DateFrom.ToString("yyyy-MM-dd") + " " + createNotificationTaskDto.TimeFrom, "yyyy-MM-dd HH:mm", Common.RussianCultureInfo),
                UserId = createNotificationTaskDto.UserId,
                CarTypes = createNotificationTaskDto.CarTypes,
                NumberSeats = createNotificationTaskDto.NumberSeats,
                Created = Common.MoscowNow,
                Updated = Common.MoscowNow,
                IsActual = true,
                IsWorked = false,
                IsStopped = false
            };

            notificationTask = await _notificationTaskRepository.FillStationCodesAsync(notificationTask);

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

            var result = notificationTasks.Select(notificationTask => new NotificationTaskDto
            {
                Id = notificationTask.Id,
                ArrivalStation = notificationTask.ArrivalStation,
                DepartureStation = notificationTask.DepartureStation,
                DateFromString = notificationTask.DepartureDateTime.ToString("dd.MM.yyyy HH:mm", Common.RussianCultureInfo),
                CarTypes = string.Join(", ", notificationTask.CarTypes.Select(c => c.GetEnumDescription())),
                NumberSeats = notificationTask.NumberSeats,
                Updated = $"{notificationTask.Updated:t}",
                TrainNumber = notificationTask.TrainNumber
            })
            .OrderBy(notificationTask => notificationTask.Id)
            .ToList();

            return result;
        }

        public async Task<IReadOnlyCollection<string>> GetPopularCitiesByUserAsync(long userId)
        {
            var topCitiesMock = new List<string>{ "Москва", "Санкт-Петербург", "Казань", "Курск" };
            
            var topCitiesByUser = await _notificationTaskRepository.GetPopularCitiesByUserAsync(userId);

            return topCitiesByUser.Count > 0 
                ? topCitiesByUser 
                : topCitiesMock;
        }
    }
}
