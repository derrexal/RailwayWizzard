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
        
        private readonly ILogger _logger;           

        /// <summary>
        /// Initializes a new instance of the <see cref="B2BService" class./>
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
            _stationsByNameService = stationsByNameService;
            _stationInfoRepository = stationInfoRepository;
            _dataExtractor = dataExtractor;
            _logger = logger;
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
        public async Task<IReadOnlyCollection<StationInfo>> StationValidateAsync(string stationName)
        {
            //Ищем станцию по полному соответствию
            var station = await GetStationInfoAsync(stationName);
            if (station is not null) return new List<StationInfo> { station };

            //Ищем станцию по НЕполному соответствию
            var stations = await GetStationsInfoAsync(stationName);

            return stations;
        }

        // Переделал. Получаю ответ только из БД.
        private async Task<StationInfo?> GetStationInfoAsync(string stationName)
        {
            // Если есть в БД
            var stationInfo = await _stationInfoRepository.FindByNameAsync(stationName);
            if (stationInfo is not null) 
                return stationInfo;

            // Идем к АПИ чтобы наполнить базу
            var stations = await GetStationsAsync(stationName);
            if (stations.Count == 0) 
                return null;
             
            // Снова смотрим есть ли в БД
            // Зачем снова идти в БД? GetStationsAsync присылает информацию о нескольких станциях, а мы хотим найти одну.
            stationInfo = await _stationInfoRepository.FindByNameAsync(stationName);
            return stationInfo ?? null;
        }

        /// <summary>
        /// Получение станций по НЕполному совпадению
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        private async Task<IReadOnlyCollection<StationInfo>> GetStationsInfoAsync(string stationName)
        {
            //Ищем в БД
            var stationsInfo = await _stationInfoRepository.ContainsByStationNameAsync(stationName);
            if (stationsInfo.Any()) 
                return stationsInfo;

            //Ищем в данных полученных по АПИ
            var stations = await GetStationsAsync(stationName);
            if (stations.Count == 0) 
                return new List<StationInfo>();

            return stations
                .Where(s => s.n.ToUpper().Contains(stationName.ToUpper()))
                .Select(s => new StationInfo { ExpressCode = s.c, Name = s.n })
                .ToList();
        }

        private async Task<IReadOnlyCollection<StationFromJson>> GetStationsAsync(string inputStation)
        {
            var textResponse = await _stationsByNameService.GetDataAsync(inputStation);
            if (string.IsNullOrEmpty(textResponse)) 
                return new List<StationFromJson>();
            
            var stations = DeserializeStationsText(textResponse);
            if (stations.Count > 0) 
                await CreateStationsInfoAsync(stations);
            
            return stations;
        }
        private static List<StationFromJson> DeserializeStationsText(string textResponse)
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
                        Name = rootStation.n,
                    });
                }
            }

            await _stationInfoRepository.AddRangeStationInfoAsync(addedStationInfo);
        }
    }
}
