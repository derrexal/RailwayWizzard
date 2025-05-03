using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RailwayWizzard.Common;
using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Infrastructure.Repositories.StationsInfo;
// using RailwayWizzard.Rzd.ApiClient.Services.GetFirewallTokenService;
using RailwayWizzard.Rzd.ApiClient.Services.GetStationDetailsService;
using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService;
using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService.Models;
using RailwayWizzard.Rzd.DataEngine.Core;

namespace RailwayWizzard.Rzd.DataEngine.Services
{
    /// <inheritdoc/>
    public class DataExtractor : IDataExtractor
    {
        private readonly IGetTrainInformationService _trainInformationService;
        private readonly IGetStationDetailsService _stationDetailsService;
        // private readonly IGetFirewallTokenService _tokenService;

        private readonly IStationInfoRepository _stationInfoRepository;
        
        private readonly ILogger<DataExtractor> _logger;

        // private static string _prevToken = string.Empty;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DataExtractor"/> class.
        /// </summary>
        /// <param name="trainInformationService">Сервис получения информации о запрашиваемом рейсе.</param>
        /// <param name="stationDetailsService">Сервис получения информации о станциях по имени.</param>
        /// <param name="stationInfoRepository"></param>
        /// <param name="logger">Логер.</param>
        public DataExtractor(
            IGetTrainInformationService trainInformationService, 
            IGetStationDetailsService stationDetailsService,
            // IGetFirewallTokenService tokenService,
            IStationInfoRepository stationInfoRepository,
            ILogger<DataExtractor> logger)
        {
            _trainInformationService = trainInformationService;
            _stationDetailsService = stationDetailsService;
            // _tokenService = tokenService;
            _stationInfoRepository = stationInfoRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> FindFreeSeatsAsync(NotificationTask task)
        {
            // string token;
            // try
            // {
            //     token = await _tokenService.GetDataAsync();
            //     _prevToken = token;
            // }
            // catch (TaskCanceledException)
            // {
            //     if(!_prevToken.Equals(string.Empty))
            //         token = _prevToken;
            //     else
            //         throw;
            // }
            
            var departureStation = await _stationInfoRepository.GetByIdAsync(task.DepartureStationId);
            var arrivalStation = await _stationInfoRepository.GetByIdAsync(task.ArrivalStationId);
            
            var trainInfoRequest = new GetTrainInformationRequest(
                departureStation.ExpressCode,
                arrivalStation.ExpressCode,
                task.DepartureDateTime,
                // token,
                false);
            var trainInfoResponse = await _trainInformationService.GetDataAsync(trainInfoRequest);

            /// Билеты перестают продавать за определенное время. Обрабатываем эти и другие ситуации.
            
            //TODO: вынести в Exceptions
            //TODO: Переделать на определение состояния по Code
            const string noTrainsMessage = "В запрашиваемую дату поездов нет";
            if (trainInfoResponse.Contains(noTrainsMessage))
            {
                _logger.LogWarning(
                    $"Task ID: {task.Id} Сервис РЖД при запросе списка свободных мест вернул ошибку: {noTrainsMessage}. " +
                    $"Ответ:{trainInfoResponse}");
                return string.Empty;
            }
            
            const string noPlaceMessage = "МЕСТ НЕТ";
            if (trainInfoResponse.Contains(noPlaceMessage))
            {
                _logger.LogWarning(
                    $"Task ID: {task.Id} Сервис РЖД при запросе списка свободных мест вернул ошибку: {noPlaceMessage}. " +
                    $"Ответ:{trainInfoResponse}");
                return string.Empty;
            }

            const string trainNotRunMessage = "В УКАЗАННУЮ ДАТУ ПОЕЗД НЕ ХОДИТ";
            if (trainInfoResponse.Contains(trainNotRunMessage))
            {
                _logger.LogWarning(
                    $"Task ID: {task.Id} Сервис РЖД при запросе списка свободных мест вернул ошибку: {trainNotRunMessage}. " +
                    $"Ответ:{trainInfoResponse}");
                return string.Empty;
            }

            var myDeserializedClass = JsonConvert.DeserializeObject<RootShort>(trainInfoResponse);
            if (myDeserializedClass?.Id == null)
                throw new NullReferenceException(
                    $"Task ID: {task.Id} Сервис РЖД при запросе списка свободных мест вернул не стандартный ответ. " +
                    $"Ответ:{trainInfoResponse}");
            if (myDeserializedClass.Trains.Count == 0)
            {
                _logger.LogError(
                    $"Task ID: {task.Id} Сервис РЖД при запросе списка свободных мест вернул ответ в котором нет доступных поездок. " +
                    $"Ответ:{trainInfoResponse}");
                return string.Empty;
            }

            task.TrainNumber ??= GetTrainNumberFromResponse(myDeserializedClass, $"{task.DepartureDateTime:t}");

            var currentRoute = ExtractFreeSeatsFromResponse(myDeserializedClass, task);
            if (currentRoute.Count == 0 || currentRoute.Sum(x => x.TotalPlace) < task.NumberSeats) 
                return string.Empty;
            
            var freeSeats = SupportingMethod(currentRoute);

            return freeSeats.Count != 0
                ? string.Join("\n", freeSeats.ToArray())
                : "";
        }

        /// <summary>
        /// Возвращает номер поезда.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="timeFrom"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        private static string? GetTrainNumberFromResponse(RootShort root, string timeFrom)
        {
            return (
                from train in root.Trains 
                where train.LocalDepartureDateTime.ToString()!.Contains(timeFrom) 
                      || train.DepartureDateTime.ToString()!.Contains(timeFrom) 
                select train.DisplayTrainNumber)
                .FirstOrDefault();
        }

        private static string GetKeyByCarType(CarType carType)
        {
            // Карта текстов и соответствующих типов
            var mappings = new Dictionary<string, IEnumerable<CarType>>
            {
                ["Sedentary"] = new[] { CarType.Sedentary, CarType.SedentaryBusiness },
                ["ReservedSeat"] = new[] { CarType.ReservedSeatLower, CarType.ReservedSeatUpper,
                                            CarType.ReservedSeatLowerSide, CarType.ReservedSeatUpperSide },
                ["Compartment"] = new[] { CarType.CompartmentLower, CarType.CompartmentUpper },
                ["Luxury"] = new[] { CarType.Luxury }
            };

            foreach (var mapping in mappings.Where(mapping => mapping.Value.Contains(carType)))
                return mapping.Key;
            
            throw new ArgumentException($"CarTypeEnum '{carType}' не соответствует ни одному ключу.");
        }

        /// <summary>
        /// Получение информации о свободных местах в запрашиваемый день по запрашиваемому рейсу
        /// </summary>
        /// <param name="root"></param>
        /// <param name="task"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        private static HashSet<SearchResult> ExtractFreeSeatsFromResponse(RootShort root, NotificationTask task)
        {
            HashSet<SearchResult> results = new();

            var currentTrain = root.Trains.FirstOrDefault(x => 
                x.LocalDepartureDateTime.ToString()!.Contains($"{task.DepartureDateTime:t}"));
            
            if (currentTrain == null)
                return results;

            foreach (var carType in task.CarTypes)
            {
                var currentCarGroups = FilterCarGroups(currentTrain.CarGroups, carType);

                var sumFreeSeats = CalculateFreeSeats(currentCarGroups, carType);

                if (sumFreeSeats == 0)
                    continue;
                
                AddOrUpdateSearchResults(results, new SearchResult
                {
                    CarType = carType.GetEnumDescription(),
                    Price = currentCarGroups.Min(x => x.MinPrice),
                    TotalPlace = sumFreeSeats,
                });
            }
            
            return results;
        }

        private static IReadOnlyCollection<CarGroupShort> FilterCarGroups(IEnumerable<CarGroupShort> carGroups, CarType carType)
        {
            var carTypeText = GetKeyByCarType(carType);

            carGroups = carType switch
            {
                CarType.Sedentary => carGroups.Where(x => !x.HasPlacesForBusinessTravelBooking),
                CarType.SedentaryBusiness => carGroups.Where(x => 
                    x.HasPlacesForBusinessTravelBooking || x.ServiceClassNameRu == carType.GetEnumDescription()),
                _ => carGroups
            };

            return carGroups
                .Where(x => x.CarType == carTypeText)
                .Where(x => !x.HasPlacesForDisabledPersons)
                .ToList();
        }

        /// <summary>
        /// Подсчитывает количество свободных мест в зависимости от типа вагона.
        /// </summary>
        private static int CalculateFreeSeats(IReadOnlyCollection<CarGroupShort> carGroups, CarType carType) =>
            carType switch
            {
                CarType.Sedentary or CarType.SedentaryBusiness or CarType.Luxury => carGroups.Sum(x => x.TotalPlaceQuantity),
                CarType.ReservedSeatUpper or CarType.CompartmentUpper => carGroups.Sum(x => x.UpperPlaceQuantity),
                CarType.ReservedSeatLower or CarType.CompartmentLower => carGroups.Sum(x => x.LowerPlaceQuantity),
                CarType.ReservedSeatUpperSide => carGroups.Sum(x => x.UpperSidePlaceQuantity),
                CarType.ReservedSeatLowerSide => carGroups.Sum(x => x.LowerSidePlaceQuantity),
                _ => throw new ArgumentOutOfRangeException(nameof(carType), $"Unexpected car type: {carType}")
            };


        /// <summary>
        /// Добавляет SearchResult в коллекцию, а если он уже добавлен - обновляет поля
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="newItem"></param>
        private static void AddOrUpdateSearchResults(HashSet<SearchResult> collection, SearchResult newItem)
        {
            if (collection.TryGetValue(newItem, out var existingItem))
                existingItem.TotalPlace += newItem.TotalPlace;
            else
                collection.Add(newItem);
        }

        private static List<string> SupportingMethod(HashSet<SearchResult> currentRoutes)
        {
            List<string> result = new();

            foreach (var route in currentRoutes)
            {
                var price = route.Price is not null
                    ? $"Цена: <strong> {Math.Round((decimal)route.Price!)}  ₽ </strong>\n"
                    : "";

                result.Add(
                    $"Класс обслуживания: <strong>{route.CarType}</strong>\n" +
                    $"Свободных мест: <strong>{route.TotalPlace}</strong>\n" +
                    price);
            }

            return result;
        }

        public async Task<string?> GetLinkToBuyTicketAsync(NotificationTask task)
        {
            const string baseLink = "https://ticket.rzd.ru/searchresults/v/1";

            try
            {
                var departureStation = await _stationInfoRepository.GetByIdAsync(task.DepartureStationId);
                var arrivalStation = await _stationInfoRepository.GetByIdAsync(task.ArrivalStationId);
                
                var departureStationNodeIdResponse = await _stationDetailsService.GetDataAsync(departureStation.Name);
                var arrivalStationNodeIdResponse = await _stationDetailsService.GetDataAsync(arrivalStation.Name);

                var departureStationNodeId = GetNodeIdStation(departureStationNodeIdResponse);
                var arrivalStationNodeId = GetNodeIdStation(arrivalStationNodeIdResponse);

                if (departureStationNodeId == string.Empty || arrivalStationNodeId == string.Empty)
                    return null;

                var dateFromText = task.DepartureDateTime.ToString("yyyy-MM-dd");

                return task.TrainNumber is null
                    ? $"{baseLink}/{departureStationNodeId}/{arrivalStationNodeId}/{dateFromText}"
                    : $"{baseLink}/{departureStationNodeId}/{arrivalStationNodeId}/{dateFromText}?trainNumber={task.TrainNumber}";
            }

            catch (Exception ex)
            {
                _logger.LogError($"Task ID: {task.Id} В ходе запроса к РЖД для получения кода станции возникла ошибка {ex}");
                return null;
            }
        }

        /// <summary>
        /// Метод получения nodeId станции из ответа АПИ.
        /// </summary>
        /// <param name="response">Ответ АПИ.</param>
        /// <returns>nodeId</returns>
        private static string GetNodeIdStation(string response)
        {
            var stationRoot = JsonConvert.DeserializeObject<StationRoot>(response);

            var city = stationRoot?.city?.FirstOrDefault();
            var train = stationRoot?.train?.FirstOrDefault();
            var avia = stationRoot?.avia?.FirstOrDefault();

            return city?.nodeId ?? train?.nodeId ?? avia?.nodeId ??
                throw new NullReferenceException(
                    $"Сервис РЖД при запросе информации о станции вернул не стандартный ответ. Ответ:{response}");
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<string>> GetAvailableTimesAsync(GetTrainInformationRequest request)
        {
            // var token = await _tokenService.GetDataAsync();
            // request = request with { FirewallToken = token };

            var trainInfoResponse = await _trainInformationService.GetDataAsync(request);

            var trainInfo = JsonConvert.DeserializeObject<RootDepartureTime>(trainInfoResponse);

            if (trainInfo == null)
                throw new NullReferenceException(
                    $"Сервис РЖД при запросе списка свободных мест " +
                    $"по рейсу {request.DepartureStationCode} - {request.ArrivalStationCode} {request.DepartureDateTime} " +
                    $"вернул не стандартный ответ. Ответ:{trainInfoResponse}");

            if (trainInfo.Trains.Count == 0)
                return new List<string>();

            var dateTimes = trainInfo.Trains
                .Select(x => x.LocalDepartureDateTime ?? x.DepartureDateTime)
                .OrderBy(x => x)
                .ToList();

            // Если не у каждой поездки есть время
            if (dateTimes.Count != trainInfo.Trains.Count)
                throw new Exception($"Сервис РЖД " +
                                    $"по рейсу {request.DepartureStationCode} - {request.ArrivalStationCode} {request.DepartureDateTime} " +
                                    $"вернул ответ в котором не у каждой поездки есть время. Ответ:{trainInfoResponse}");

            // Если требуется информация не на сегодня - просто отдаем
            if (request.DepartureDateTime.Date > DateTimeExtensions.MoscowNow.Date)
                return dateTimes
                    .Select(x => x.ToString("HH:mm"))
                    .ToList();

            // Иначе определяем актуальное для пользователя и отдаем
            return dateTimes
                .Where(dateTime => dateTime > DateTimeExtensions.MoscowNow)
                .Select(x => x.ToString("HH:mm"))
                .ToList();
        }
    }
}
