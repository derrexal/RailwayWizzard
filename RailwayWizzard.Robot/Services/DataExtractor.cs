using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RailwayWizzard.Common;
using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Core.TrainBotTasks;
using RailwayWizzard.Infrastructure.Repositories.StationsInfo;
using RailwayWizzard.Rzd.ApiClient.Services.GetStationDetailsService;
using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService;
using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService.Models;
using RailwayWizzard.Rzd.DataEngine.Core;
using RailwayWizzard.Rzd.DataEngine.Exceptions;

namespace RailwayWizzard.Rzd.DataEngine.Services
{
    /// <inheritdoc/>
    public class DataExtractor : IDataExtractor
    {
        private readonly IGetTrainInformationService _trainInformationService;
        private readonly IGetStationDetailsService _stationDetailsService;
        private readonly IStationInfoRepository _stationInfoRepository;
        private readonly ILogger<DataExtractor> _logger;
        
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
            IStationInfoRepository stationInfoRepository,
            ILogger<DataExtractor> logger)
        {
            _trainInformationService = trainInformationService;
            _stationDetailsService = stationDetailsService;
            _stationInfoRepository = stationInfoRepository;
            _logger = logger;
        }
        //TODO: Переделать на определение состояния по Code
        /// Билеты перестают продавать за определенное время. Обрабатываем эти и другие ситуации.
        private Exception? ProcessTrainInfoResponse(int taskId, string trainInfoResponse)
        {
            if (trainInfoResponse.Contains(TrainNotFoundException.noTrainsMessage))
            {
                return TrainNotFoundException.New(taskId);
            }
            else if (trainInfoResponse.Contains(NoPlaceException.noPlaceMessage))
            {
                return NoPlaceException.New(taskId);
            }
            else if (trainInfoResponse.Contains(DontRunException.dontRunMessage))
            {
                return DontRunException.New(taskId);
            }
            
            return null;
            
        } 
        
        // TODO: Переделать на TrainBotTask или иметь собственную удобную денормализованную сущность на уровне этого сервиса?
        private async Task<Root> GetTrainInfoAsync(NotificationTask task)
        {
            // TODO: Предлагаю денормализовывать задачу в воркере (в отдельном или в том который читает очередь и пишет в таблицу)
            // До такой степени, чтобы процесс поиска свободных мест не отвлекался на лишние запросы к БД.
            var departureStation = await _stationInfoRepository.GetByIdAsync(task.DepartureStationId);
            var arrivalStation = await _stationInfoRepository.GetByIdAsync(task.ArrivalStationId);
            
            var trainInfoRequest = new GetTrainInformationRequest(
                departureStation.ExpressCode,
                arrivalStation.ExpressCode,
                task.DepartureDateTime,
                false);
            var trainInfoResponse = await _trainInformationService.GetDataAsync(trainInfoRequest);
            
            var exception = ProcessTrainInfoResponse(task.Id, trainInfoResponse);
            if (exception != null)
            {
                throw exception;
            }
                
            var trainInfo = JsonConvert.DeserializeObject<Root>(trainInfoResponse);
            if (trainInfo?.Id == null)
                throw UnknownResponseException.New(task.Id,trainInfoResponse);
            if (trainInfo.Trains.Count == 0)
            {
                throw TrainNotFoundException.New(task.Id);
            }
            
            return trainInfo;
        }
        

        /// <inheritdoc/>
        public async Task<string> FindFreeSeatsAsync(TrainBotTask task)
        {
            var trainInfo = await GetTrainInfoAsync(task);             
            
            var currentRoute = ExtractFreeSeatsFromResponse(trainInfo, task);
            
            var freeSeats = SupportingMethod(currentRoute);
            
            return freeSeats.Count != 0
                ? string.Join("\n", freeSeats.ToArray())
                : "";
        }
        
        // Карта текстов и соответствующих типов
        private static string GetKeyByCarType(CarType carType)
        {
            // TODO: Инициализируются заново при каждом запуске функции.
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
        
        private IReadOnlyCollection<CarType> MapSeatClassCodesToCarTypes(IReadOnlyCollection<string> seatClassCodes)
        {
            var result = new HashSet<CarType>();
            if (seatClassCodes.Count == 0)
                return result;

            foreach (var code in seatClassCodes)
            {
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                switch (code.Trim().ToLowerInvariant())
                {
                    case "seat":
                        result.Add(CarType.Sedentary);
                        break;
                    case "seat_biz":
                        result.Add(CarType.SedentaryBusiness);
                        break;
                    case "platz_up":
                        result.Add(CarType.ReservedSeatUpper);
                        break;
                    case "platz_down":
                        result.Add(CarType.ReservedSeatLower);
                        break;
                    case "platz_up_side":
                        result.Add(CarType.ReservedSeatUpperSide);
                        break;
                    case "platz_down_side":
                        result.Add(CarType.ReservedSeatLowerSide);
                        break;
                    case "kupe_up":
                        result.Add(CarType.CompartmentUpper);
                        break;
                    case "kupe_down":
                        result.Add(CarType.CompartmentLower);
                        break;
                    case "sv":
                        result.Add(CarType.Luxury);
                        break;
                    default:
                        _logger.LogWarning("Unknown seat class code '{Code}' in seat_params.", code);
                        break;
                }
            }

            return result;
        }

        private List<CarGroup> ExtractCurrentCarGroups(TrainBotTask task, List<TrainInfo> trains)
        {
            var carGroups = new List<CarGroup>();

            // SeatMode
            if (task.SeatMode == TrainBotSeatMode.ByServiceClass)
            {
                var classes = ((ByServiceClassParams)task.SeatParams).Classes;
                var carTypes = MapSeatClassCodesToCarTypes(classes);

                foreach (var train in trains)
                {
                    foreach (var carType in carTypes)
                    {
                        var currentCarGroups = FilterCarGroups(train.CarGroups, carType);
                        carGroups.AddRange(currentCarGroups);
                    }
                }
            }
            
            // ПОКА НЕ УМЕЕМ ФИЛЬТР ПО НОМЕРУ ВАГОНА, текущий запрос не возвращает информацию о номере вагона.
            
            // Disables Person
            if (task.AccessibleOnly)
            {
                return carGroups
                    .Where(x => x.HasPlacesForDisabledPersons)
                    .ToList();
            }

            return carGroups
                .Where(x => !x.HasPlacesForDisabledPersons)
                .ToList();
        }
        
        private List<TrainInfo> ExtractCurrentTrains(TrainBotTask task, Root root)
        {
            var trains = new List<TrainInfo>();

            switch (task.Mode)
            {
                // Mode
                case TrainBotModeType.SpecificTrain:
                {
                    var mode = (SpecificTrainModeParams)task.ModeParams;
                    var departureTime = mode.Time.ToTimeSpan();
                
                    var currentTrain = root.Trains.FirstOrDefault(train =>
                        train.LocalDepartureDateTime!.Value.TimeOfDay.Equals(departureTime));
 
                    if (currentTrain != null)
                        trains.Add(currentTrain);
                
                    break;
                }
                case TrainBotModeType.TimeInterval:
                {
                    var (_, _, arrFrom, arrTo) = (TimeIntervalModeParams)task.ModeParams;
                    if (arrFrom == null || arrTo == null)
                    {
                        _logger.LogError("TimeIntervalModeParams not set.");
                    }

                    var currentTrains = root.Trains.Where(train =>
                        train.LocalArrivalDateTime!.Value.TimeOfDay >= arrFrom!.Value.ToTimeSpan() &&
                        train.LocalArrivalDateTime!.Value.TimeOfDay <= arrTo!.Value.ToTimeSpan());
                
                    trains.AddRange(currentTrains);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(task.Mode), task.Mode, "Некорректный параметр");
            }
            
            return trains;
        }
        
        /// <summary>
        /// Получение информации о свободных местах в запрашиваемый день по запрашиваемому рейсу
        /// </summary>
        /// <param name="root"></param>
        /// <param name="task"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        private HashSet<SearchResult> ExtractFreeSeatsFromResponse(Root root, TrainBotTask task)
        {
            var results = new HashSet<SearchResult>();
            
            var trains = ExtractCurrentTrains(task, root);
            var carGroups =  ExtractCurrentCarGroups(task, trains);
            
            // Собираем предварительные результаты.
            var classes = ((ByServiceClassParams)task.SeatParams).Classes;
            var carTypes = MapSeatClassCodesToCarTypes(classes);
            foreach (var carType in carTypes)
            {
                var sumFreeSeats = CalculateFreeSeats(carGroups, carType);
                if (sumFreeSeats == 0)
                    continue;

                AddOrUpdateSearchResults(results, new SearchResult
                {
                    CarType = carType.GetEnumDescription(),
                    Price = carGroups.Min(x => x.MinPrice),
                    TotalPlace = sumFreeSeats,
                });
            }
            
            // Seats count
            var totalPlaces = results.Sum(x => x.TotalPlace); 
            return totalPlaces < task.Seats ? new HashSet<SearchResult>() : results;
        }

        private static IReadOnlyCollection<CarGroup> FilterCarGroups(IEnumerable<CarGroup> carGroups, CarType carType)
        {
            var carTypeText = GetKeyByCarType(carType);

            carGroups = carType switch
            {
                CarType.Sedentary => carGroups.Where(x => !x.IsBusinessClass),
                CarType.SedentaryBusiness => carGroups.Where(carGroup => carGroup.IsBusinessClass),
                _ => carGroups
            };

            return carGroups
                .Where(x => x.CarType == carTypeText)
                .ToList();
        }

        /// <summary>
        /// Подсчитывает количество свободных мест в зависимости от типа вагона.
        /// </summary>
        private static int CalculateFreeSeats(IReadOnlyCollection<CarGroup> carGroups, CarType carType) =>
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

        // TODO: Тоже зачем то ходим и в БД и в АПИ РЖД именно здесь, хотя можем делать это исключительно в воркере денормализации.
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
            if (request.DepartureDateTime > DateTimeExtensions.MoscowNow.Date)
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
