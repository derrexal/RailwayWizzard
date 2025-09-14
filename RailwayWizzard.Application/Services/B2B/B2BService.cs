using Newtonsoft.Json;
using RailwayWizzard.Application.Dto.B2B;
using RailwayWizzard.Rzd.ApiClient.Services.GetStationsByNameService;
using RailwayWizzard.Common;
using RailwayWizzard.Core.StationInfo;
using RailwayWizzard.Infrastructure.Repositories.StationsInfo;
using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService.Models;
using RailwayWizzard.Rzd.DataEngine.Services;

namespace RailwayWizzard.Application.Services.B2B
{
    /// <inheritdoc/>
    public class B2BService : IB2BService
    {
        private readonly IGetStationsByNameService _stationsByNameService;
        private readonly IStationInfoRepository _stationInfoRepository;
        private readonly IDataExtractor _dataExtractor;
        
        private readonly ILogger<B2BService> _logger;           

        /// <summary>
        /// Initializes a new instance of the <see cref="B2BService"/> class.
        /// </summary>
        /// <param name="stationsByNameService">Сервис получения информации о станциях по имени.</param>
        /// <param name="stationInfoRepository"></param>
        /// <param name="dataExtractor"></param>
        /// <param name="logger"></param>
        public B2BService(
            IGetStationsByNameService stationsByNameService,
            IStationInfoRepository stationInfoRepository,
            IDataExtractor dataExtractor,
            ILogger<B2BService> logger)
        {
            _stationsByNameService = Ensure.NotNull(stationsByNameService);
            _stationInfoRepository = Ensure.NotNull(stationInfoRepository);
            _dataExtractor = Ensure.NotNull(dataExtractor);
            _logger = Ensure.NotNull(logger);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<string>> GetAvailableTimesAsync(RouteDto routeDto)
        {
            routeDto.DepartureDate.IsNotActualMoscowTime();
            
            var departureStationInfo = await _stationInfoRepository.GetByNameAsync(routeDto.DepartureStationName);
            var arrivalStationInfo = await _stationInfoRepository.GetByNameAsync(routeDto.ArrivalStationName);

            var request = new GetTrainInformationRequest
            (
                departureStationInfo.ExpressCode,
                arrivalStationInfo.ExpressCode,
                routeDto.DepartureDate
            );

            var availableTimes = await _dataExtractor.GetAvailableTimesAsync(request);

            return availableTimes; 
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<StationInfoExtended>> StationValidateAsync(string stationName)
        {
            //Ищем станцию по полному соответствию
            var station = await GetStationInfoAsync(stationName);
            if (station is not null) 
                return new List<StationInfoExtended> { station };

            //Ищем станцию по неполному соответствию
            var stations = await GetStationsInfoAsync(stationName);

            return stations;
        }

        /// <summary>
        /// Получение станции по полному совпадению.
        /// </summary>
        /// <param name="stationName">Наименование станции.</param>
        /// <returns>Модель найденной станции.</returns>
        private async Task<StationInfoExtended?> GetStationInfoAsync(string stationName)
        {
            // Сначала проверяем - может станция уже есть в БД
            var stationInfo = await _stationInfoRepository.FindByNameExactAsync(stationName);
            if (stationInfo is not null) 
                return stationInfo;

            // Идем к АПИ чтобы наполнить базу
            var stations = await GetStationsFromApiAsync(stationName);
            if (!stations.Any()) 
                return null;
             
            // Снова смотрим есть ли в БД
            // Зачем снова идти в БД? GetStationsAsync присылает информацию о нескольких станциях, а мы хотим найти одну.
            stationInfo = await _stationInfoRepository.FindByNameExactAsync(stationName);
            return stationInfo ?? null;
        }

        /// <summary>
        /// Получение станций по неполному совпадению.
        /// </summary>
        /// <param name="stationName">Наименование станции.</param>
        /// <returns>Коллекция найденных станций.</returns>
        private async Task<IReadOnlyCollection<StationInfoExtended>> GetStationsInfoAsync(string stationName)
        {
            //Ищем в БД
            var stationsInfo = await _stationInfoRepository.FindByNameContainsAsync(stationName);
            if (stationsInfo.Any()) 
                return stationsInfo;

            //Ищем в данных полученных по АПИ
            var stations = await GetStationsFromApiAsync(stationName);
            if (!stations.Any()) 
                return new List<StationInfoExtended>();

            return stations
                .Where(s => s.Name.ToUpper().Contains(stationName.ToUpper()))
                .ToList();
        }

        private async Task<IReadOnlyCollection<StationInfoExtended>> GetStationsFromApiAsync(string inputStation)
        {
            try
            {
                var textResponse = await _stationsByNameService.GetDataAsync(inputStation);
                if (string.IsNullOrEmpty(textResponse)) 
                    throw new NullReferenceException($"РЖД не нашел доступных станция по наименованию: '{inputStation}'. Ответ: {textResponse}");
            
                // TODO: Дессериализации здесь не место...
                var stations = DeserializeStationsText(textResponse);
                var stationInfos = await CreateStationsInfoAsync(stations);
            
                return stationInfos;
            }
            catch (NullReferenceException e)
            {
                _logger.LogWarning(e, "РЖД не нашел доступных станция по наименованию:'{0}'", inputStation);
                return new List<StationInfoExtended>();
            }
        }
        
        private static StationInfoFromJson DeserializeStationsText(string textResponse)
        {
            var stations = JsonConvert.DeserializeObject<StationInfoFromJson>(textResponse);
            
            if (stations is null || !stations.city.Any())
                throw new NullReferenceException($"РЖД не нашел доступных станция по наименованию. Ответ: {textResponse}");

            return stations;
        }

        /// <summary>
        /// Добавляет в таблицу AppStationInfo новые записи
        /// </summary>
        /// <param name="rootStations"></param>
        /// <returns></returns>
        private async Task<IReadOnlyCollection<StationInfoExtended>> CreateStationsInfoAsync(StationInfoFromJson rootStations)
        {
            var stationInfos = rootStations.city.Select(city => 
                    new StationInfoExtended 
                    { 
                        Name = city.name, 
                        ExpressCode = city.expressCode, 
                        NodeId = city.nodeId, 
                        NodeType = city.nodeType,
                    })
                .ToList();

            await _stationInfoRepository.AddRangeStationInfosAsync(stationInfos);            

            return stationInfos;
        }
    }
}
