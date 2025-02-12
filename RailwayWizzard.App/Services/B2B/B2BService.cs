﻿using System.Globalization;
using Abp.Collections.Extensions;
using Newtonsoft.Json;
using RailwayWizzard.App.Dto.B2B;
using RailwayWizzard.B2B;
using RailwayWizzard.B2BHelper.App;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Repositories.StationInfos;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App.Services.B2B
{
    /// <inheritdoc/>
    public class B2BService : IB2BService
    {
        private readonly IB2BClient _b2BClient;
        private readonly IStationInfoRepository _stationInfoRepository;
        private readonly IRobot _robot;
        // TODO: Почему тут просто логер а в воркере например типизированный?
        private readonly ILogger _logger;           

        /// <summary>
        /// Initializes a new instance of the <see cref="B2BService" class./>
        /// </summary>
        /// <param name="b2BClient">B2B клиент для связи с РЖД.</param>
        /// <param name="stationInfoRepository"></param>
        /// <param name="robot"></param>
        /// <param name="logger"></param>
        public B2BService(
            IB2BClient b2BClient,
            IStationInfoRepository stationInfoRepository,
            IRobot robot,
            ILogger<B2BService> logger)
        {
            _b2BClient = b2BClient;
            _stationInfoRepository = stationInfoRepository;
            _robot = robot;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<string>> GetAvailableTimesAsync(RouteDto routeDto)
        {
            //TODO: Вынести в метод расширения DateTime
            if(routeDto.DepartureDate.Date < Common.MoscowNow.Date)
                throw new ArgumentException("Invalid route argument. Date is before moscow");
            
            var departureStationInfo = await _stationInfoRepository.FindByStationNameAsync(routeDto.DepartureStationName);
            var arrivalStationInfo = await _stationInfoRepository.FindByStationNameAsync(routeDto.ArrivalStationName);
            
            //TODO: Создать новый метод Get внутри которого будет проброс ошибки 
            if (departureStationInfo is null || arrivalStationInfo is null)
                throw new ArgumentException($"Не найдена одна из станций: {routeDto.DepartureStationName},{routeDto.ArrivalStationName}. Вероятно в названии допущена ошибка");

            var notificationTask = new NotificationTask
            {
                DepartureDateTime = routeDto.DepartureDate,
                DepartureStation = routeDto.DepartureStationName,
                ArrivalStation = routeDto.ArrivalStationName,
                DepartureStationCode = departureStationInfo.ExpressCode,
                ArrivalStationCode = arrivalStationInfo.ExpressCode,
            };

            var availableTimes = await _robot.GetAvailableTimesAsync(notificationTask);

            return availableTimes;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<StationInfo>> StationValidateAsync(string stationName)
        {
            //Ищем станцию по полному соответствию
            var station = await GetStationInfoAsync(stationName);
            if (station is not null) return new List<StationInfo> { station };

            //Ищем станцию по НЕполному соответствию
            var stations = await GetStationsInfo(stationName);

            return stations;
        }

        // Переделал. Получаю ответ только из БД.
        private async Task<StationInfo?> GetStationInfoAsync(string stationName)
        {
            // Если есть в БД
            var stationInfo = await _stationInfoRepository.FindByStationNameAsync(stationName);
            if (stationInfo is not null) { return stationInfo; }

            // Идем к АПИ чтобы наполнить базу
            var stations = await GetStations(stationName);
            // И у АПИ не нашли 
            if (stations.Count == 0) return null;

            // Снова смотрим есть ли в БД
            stationInfo = await _stationInfoRepository.FindByStationNameAsync(stationName);
            return stationInfo ?? null;
        }

        /// <summary>
        /// Получение станций по НЕполному совпадению
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        private async Task<IReadOnlyCollection<StationInfo>> GetStationsInfo(string stationName)
        {
            //Ищем в БД
            var stationsInfo = await _stationInfoRepository.ContainsByStationNameAsync(stationName);
            if (stationsInfo.Any()) return stationsInfo;

            //Ищем в данных полученных по АПИ
            var stations = await GetStations(stationName);
            if (stations.Count == 0) return new List<StationInfo>();
            return stations
                .Where(s => s.n.ToUpper().Contains(stationName.ToUpper()))
                .Select(s => new StationInfo { ExpressCode = s.c, StationName = s.n })
                .ToList();
        }

        private async Task<IReadOnlyCollection<StationFromJson>> GetStations(string inputStation)
        {
            var textResponse = await _b2BClient.GetStationsText(inputStation);
            if (textResponse.IsNullOrEmpty()) { return new List<StationFromJson>(); }
            var stations = DeserializeStationsText(textResponse);

            if (stations.Count > 0) await CreateStationsInfoAsync(stations);
            return stations;
        }
        private List<StationFromJson> DeserializeStationsText(string textResponse)
        {
            var stations = JsonConvert.DeserializeObject<List<StationFromJson>>(textResponse);
            if (stations!.Count == 0)
                throw new NullReferenceException(
                    $"Сервис РЖД при запросе списка свободных мест вернул ответ в котором нет поездок. Ответ:{textResponse}");

            return stations;
        }

        /// <summary>
        /// Добавляет в таблицу AppStationInfo новые записи
        /// </summary>
        /// <param name="rootStations"></param>
        /// <returns></returns>
        private async Task CreateStationsInfoAsync(IReadOnlyCollection<StationFromJson> rootStations)
        {
            var addedStationInfo = new List<StationInfo>();

            foreach (var rootStation in rootStations)
            {
                var anyStationInfo = await _stationInfoRepository.AnyByExpressCodeAsync(rootStation.c);

                if (anyStationInfo is false)
                {
                    addedStationInfo.Add(new StationInfo
                    {
                        ExpressCode = rootStation.c,
                        StationName = rootStation.n,
                    });
                }
            }

            await _stationInfoRepository.AddRangeStationInfoAsync(addedStationInfo);
        }
    }
}
