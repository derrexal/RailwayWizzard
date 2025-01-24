using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RailwayWizzard.B2B;
using RailwayWizzard.B2BHelper.Core;
using RailwayWizzard.Core;
using RailwayWizzard.Robot.Core;
using RailwayWizzard.Shared;

namespace RailwayWizzard.B2BHelper.App
{
    /// <inheritdoc/>
    public class RobotBigBrother : IRobot
    {
        private readonly ILogger<RobotBigBrother> _logger;
        private readonly IB2BClient _b2BClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotBigBrother"/> class.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="b2BClient">B2B клиент.</param>
        public RobotBigBrother(ILogger<RobotBigBrother> logger, IB2BClient b2BClient)
        {
            _logger = logger;
            _b2BClient = b2BClient;
        }

        /// <inheritdoc/>
        public async Task<string> GetFreeSeatsOnTheTrain(NotificationTask inputNotificationTask)
        {
            var ksid = await GetKsidForGetTicketAsync();

            // TODO: раз уж здесь мы получаем только поезда в которых билеты есть? может можно упростить дальнейшую проверку? ExtractFreeSeatsFromResponse
            var textResponse = await _b2BClient.GetTrainInformationByParametersAsync(inputNotificationTask, ksid, false);

            /// Билеты перестают продавать за определенное время. Обрабатываем эти ситуации.
            const string noPlaceMessage = "МЕСТ НЕТ";
            if (textResponse.Contains(noPlaceMessage))
            {
                _logger.LogWarning($"Сервис РЖД при запросе списка свободных мест вернул ошибку с текстом: {noPlaceMessage}. Ответ:{textResponse}");
                return string.Empty;
            }

            const string trainNotRunMessage = "В УКАЗАННУЮ ДАТУ ПОЕЗД НЕ ХОДИТ";
            if (textResponse.Contains(trainNotRunMessage))
            {
                _logger.LogWarning($"Сервис РЖД при запросе списка свободных мест вернул ошибку с текстом: {trainNotRunMessage}. Ответ:{textResponse}");
                return string.Empty;
            }

            var myDeserializedClass = JsonConvert.DeserializeObject<RootShort>(textResponse);
            if (myDeserializedClass?.Id == null)
                throw new NullReferenceException($"Сервис РЖД при запросе списка свободных мест вернул не стандартный ответ. Ответ:{textResponse}");
            if (myDeserializedClass.Trains.Count == 0)
            {
                _logger.LogError($"Сервис РЖД при запросе списка свободных мест вернул ответ в котором нет доступных поездок. Ответ:{textResponse}");
                return string.Empty;
            }

            inputNotificationTask.TrainNumber ??= GetTrainNumberFromResponse(
                myDeserializedClass,
                $"{inputNotificationTask.DepartureDateTime:t}");

            //вытаскиваем свободные места из полученного ответа
            var currentRoute = ExtractFreeSeatsFromResponse(myDeserializedClass, inputNotificationTask);
            if (currentRoute.Count == 0 || currentRoute.Count < inputNotificationTask.NumberSeats) 
                return string.Empty;
            
            //Формируем текстовый ответ пользователю
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

        private static string GetKeyByCarType(CarTypeEnum carType)
        {
            // Карта текстов и соответствующих типов
            var mappings = new Dictionary<string, IEnumerable<CarTypeEnum>>
            {
                ["Sedentary"] = new[] { CarTypeEnum.Sedentary, CarTypeEnum.SedentaryBusiness },
                ["ReservedSeat"] = new[] { CarTypeEnum.ReservedSeatLower, CarTypeEnum.ReservedSeatUpper,
                                            CarTypeEnum.ReservedSeatLowerSide, CarTypeEnum.ReservedSeatUpperSide },
                ["Compartment"] = new[] { CarTypeEnum.CompartmentLower, CarTypeEnum.CompartmentUpper },
                ["Luxury"] = new[] { CarTypeEnum.Luxury }
            };

            foreach (var mapping in mappings.Where(mapping => mapping.Value.Contains(carType)))
                return mapping.Key;
            
            throw new ArgumentException($"CarTypeEnum '{carType}' не соответствует ни одному ключу.");
        }

        /// <summary>
        /// Получение информации о свободных местах в запрашиваемый день по запрашиваемому рейсу
        /// </summary>
        /// <param name="root"></param>
        /// <param name="inputNotificationTask"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        private static HashSet<SearchResult> ExtractFreeSeatsFromResponse(RootShort root, NotificationTask inputNotificationTask)
        {
            HashSet<SearchResult> results = new();

            var currentTrain = root.Trains.FirstOrDefault(x => 
                x.LocalDepartureDateTime.ToString()!.Contains($"{inputNotificationTask.DepartureDateTime:t}"));
            
            if (currentTrain == null)
                return results;

            foreach (var carType in inputNotificationTask.CarTypes)
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

        private static IReadOnlyCollection<CarGroupShort> FilterCarGroups(IEnumerable<CarGroupShort> carGroups, CarTypeEnum carType)
        {
            var carTypeText = GetKeyByCarType(carType);

            carGroups = carType switch
            {
                CarTypeEnum.Sedentary => carGroups.Where(x => !x.HasPlacesForBusinessTravelBooking),
                CarTypeEnum.SedentaryBusiness => carGroups.Where(x => 
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
        private static int CalculateFreeSeats(IReadOnlyCollection<CarGroupShort> carGroups, CarTypeEnum carType) =>
            carType switch
            {
                CarTypeEnum.Sedentary or CarTypeEnum.SedentaryBusiness or CarTypeEnum.Luxury => carGroups.Sum(x => x.TotalPlaceQuantity),
                CarTypeEnum.ReservedSeatUpper or CarTypeEnum.CompartmentUpper => carGroups.Sum(x => x.UpperPlaceQuantity),
                CarTypeEnum.ReservedSeatLower or CarTypeEnum.CompartmentLower => carGroups.Sum(x => x.LowerPlaceQuantity),
                CarTypeEnum.ReservedSeatUpperSide => carGroups.Sum(x => x.UpperSidePlaceQuantity),
                CarTypeEnum.ReservedSeatLowerSide => carGroups.Sum(x => x.LowerSidePlaceQuantity),
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

        private async Task<string?> GetLinkToBuyTicket(NotificationTask notificationTask)
        {
            const string baseLink = "https://ticket.rzd.ru/searchresults/v/1";

            try
            {
                var departureStationNodeIdResponse = await _b2BClient.GetNodeIdStationAsync(notificationTask.DepartureStation);
                var arrivalStationNodeResponse = await _b2BClient.GetNodeIdStationAsync(notificationTask.ArrivalStation);

                var departureStationNodeId = GetNodeIdStation(departureStationNodeIdResponse);
                var arrivalStationNodeId = GetNodeIdStation(arrivalStationNodeResponse);

                if (departureStationNodeId == string.Empty || arrivalStationNodeId == string.Empty)
                    return null;

                var dateFromText = notificationTask.DepartureDateTime.ToString("yyyy-MM-dd");

                return notificationTask.TrainNumber is null
                    ? $"{baseLink}/{departureStationNodeId}/{arrivalStationNodeId}/{dateFromText}"
                    : $"{baseLink}/{departureStationNodeId}/{arrivalStationNodeId}/{dateFromText}?trainNumber={notificationTask.TrainNumber}";

            }
            catch (Exception ex)
            {
                _logger.LogError($"В ходе запроса к РЖД для получения кода станции возникла ошибка {ex}");
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

            if (stationRoot == null || stationRoot.city.Count == 0)
                throw new NullReferenceException($"Сервис РЖД при запросе информации о станции вернул не стандартный ответ. Ответ:{response}");

            var city = stationRoot.city.FirstOrDefault();
            if (city == null)
                throw new NullReferenceException($"Сервис РЖД при запросе информации о станции вернул не стандартный ответ. Ответ:{response}");

            return city.nodeId ?? "";
        }

        private async Task<string> GetKsidForGetTicketAsync()
        {
            try
            {
                var textResponse = await _b2BClient.GetKsidAsync();

                //вытаскиваем из ответа строку со Ksid
                if (!textResponse.Contains("id")) throw new HttpRequestException($"Сервис Касперского вернул невалидный ответ:\n{textResponse}");
                var regex = new Regex("\"id\":\"(.*?)\"");
                var res = regex.Match(textResponse).ToString();

                //Из всей строки получаем только значение
                var keyValuePairs = res.Split(':').ToList();
                var result = keyValuePairs.LastOrDefault();
                if (result is null)
                    throw new HttpRequestException($"Не удалось распарсить ответ от Касперского:\n{textResponse}");

                result = result.Remove(result.Length - 1);
                result = result.Remove(0, 1);
                return result;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Не доступен сервис Касперского для получения токена\n{e}");
                throw;
            }
        }

        // TODO: вынести этот код в другой сервис! Это не относится к сервису получения свободных мест.
        public string GetMessageSeatsIsEmpty(NotificationTask notificationTask)
        {
            return $"{char.ConvertFromUtf32(0x26D4)} " +
                $"{notificationTask.ToBotString()}" +
                "\nСвободных мест больше нет";
        }

        public async Task<string> GetMessageSeatsIsComplete(NotificationTask notificationTask, string resultFreeSeats)
        {
            var linkToBuyTicketResponse = await GetLinkToBuyTicket(notificationTask);

            var linkToBuyTicket = linkToBuyTicketResponse is not null
                ? $"\n<a href=\"{linkToBuyTicketResponse}\">Купить билет</a>"
                  : "";

            var result =
                $"{char.ConvertFromUtf32(0x2705)} {notificationTask.ToBotString()}" +
                $"\n\n{resultFreeSeats}" +
                linkToBuyTicket;

            return result;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<string>> GetAvailableTimesAsync(NotificationTask notificationTask)
        {
            var ksid = await GetKsidForGetTicketAsync();

            var textResponse = await _b2BClient.GetTrainInformationByParametersAsync(notificationTask, ksid);

            var root = JsonConvert.DeserializeObject<RootDepartureTime>(textResponse);

            if (root == null || root.Trains.Count == 0)
                throw new NullReferenceException($"Сервис РЖД при запросе списка свободных мест вернул не стандартный ответ. Ответ:{textResponse}");

            var dateTimes = root.Trains
                .Select(x => x.LocalDepartureDateTime ?? x.DepartureDateTime)
                .OrderBy(x=>x)
                .ToList();
            
            /// Если не у каждой поездки есть время
            if (dateTimes.Count != root.Trains.Count)
                throw new Exception($"Сервис РЖД вернул ответ в котором не у каждой поездки есть время. Ответ:{textResponse}");

            // Если требуется информация не на сегодня - просто отдаем
            if (notificationTask.DepartureDateTime.Date > Common.MoscowNow.Date) 
                return GetTimesText(dateTimes);

            // Иначе определяем актуальное для пользователя и отдаем
            var availableDateTimes = GetAvailableTimes(dateTimes);
            return GetTimesText(availableDateTimes);
        }

        private static IReadOnlyCollection<string> GetTimesText(List<DateTime> dateTimes) =>
            dateTimes.Select(x => x.ToString("HH:mm")).ToList();

        /// <summary>
        /// Возвращает доступное время.
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        private static List<DateTime> GetAvailableTimes(List<DateTime> dateTimes)
        {
            var moscowDateTime = Common.MoscowNow;

            return dateTimes.Where(dateTime => dateTime > moscowDateTime).ToList();
        }
    }
}
