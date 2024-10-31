using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RailwayWizzard.B2B;
using RailwayWizzard.Core;
using RailwayWizzard.Robot.Core;
using System.Text.RegularExpressions;

namespace RailwayWizzard.Robot.App
{
    public class RobotBigBrother : IRobot
    {
        private readonly ILogger<RobotBigBrother> _logger;
        private readonly IB2BClient _b2bClient;

        public RobotBigBrother(ILogger<RobotBigBrother> logger, IB2BClient b2bClient)
        {
            _logger = logger;
            _b2bClient = b2bClient;
        }

        /// <inheritdoc/>
        // TODO: избавиться от этого метода - сделать RetryPolicy
        public async Task<string> GetFreeSeatsOnTheTrain(NotificationTask inputNotificationTask)
        {
            const int retryCount = 3;

            var freeSeats = await GetFreeSeatsOnTheTrainHelper(inputNotificationTask);

            for (int i = 0; i < retryCount; i++)
            {
                if (freeSeats.Count == 0)
                {
                    await Task.Delay(5000);

                    freeSeats = await GetFreeSeatsOnTheTrainHelper(inputNotificationTask);
                }
                else
                {
                    break;
                }
            }

            return freeSeats.Count != 0 
                ? String.Join("\n", freeSeats.ToArray())
                : "";
        }

        private async Task<List<string>> GetFreeSeatsOnTheTrainHelper(NotificationTask inputNotificationTask)
        {
            string ksid = await GetKsidForGetTicketAsync();

            var textResponse = await _b2bClient.GetTrainInformationByParametersAsync(inputNotificationTask, ksid);
            //TODO: нужно смапить в DTO чтобы этот огромный объект не таскать по памяти
            RootBigBrother? myDeserializedClass = JsonConvert.DeserializeObject<RootBigBrother>(textResponse);

            if (myDeserializedClass == null || myDeserializedClass.Id == null)
                throw new NullReferenceException($"Сервис РЖД при запросе списка свободных мест вернул не стандартный ответ. Ответ:{textResponse}");
            if (myDeserializedClass.Trains.Count == 0)
            {
                _logger.LogError($"Сервис РЖД при запросе списка свободных мест вернул ответ в котором нет доступных поездок. Ответ:{textResponse}");
                return new List<string>();
            }
            //TODO: Если места были, затем РЖД ответил с Trains=null - в ответ пользователь получит сообщение о том что мест нет без номера поезда. Хранить в БД.
            inputNotificationTask.TrainNumber = GetTrainNumberFromResponse(myDeserializedClass, inputNotificationTask.TimeFrom);

            //вытаскиваем свободные места по запрашиваемому рейсу
            var currentRoute = GetCurrentRouteFromResponse(myDeserializedClass, inputNotificationTask);
            if (currentRoute.Count == 0) return new List<string>();

            //Формируем текстовый ответ пользователю
            var result = SupportingMethod(currentRoute);

            return result;
        }

        /// <summary>
        /// Возвращает номер поезда.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="TimeFrom"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public string? GetTrainNumberFromResponse(RootBigBrother root, string TimeFrom)
        {
            foreach (var train in root.Trains)
                if (train.LocalDepartureDateTime.ToString()!.Contains(TimeFrom)) //Если в ответе содержится необходимая поездка
                    return train.DisplayTrainNumber;
            return null;
        }

        /// <summary>
        /// Получение информации о свободных местах в запрашиваемый день по запрашиваемому рейсу
        /// </summary>
        /// <param name="root"></param>
        /// <param name="departureTime"></param>
        /// <returns></returns>
        private HashSet<SearchResult> GetCurrentRouteFromResponse(RootBigBrother root, NotificationTask inputNotificationTask)
        {
            HashSet<SearchResult> results = new();

            //todo: костыль, но как по другому?
            //наполяем список типов вагонов словами, чтобы было с чем сравнивать
            List<string> carTypesText = new List<string>();
            foreach (var carType in inputNotificationTask.CarTypes)
                switch (carType)
                {
                    case CarTypeEnum.Sedentary:
                        carTypesText.Add("Sedentary");
                        break;
                    case CarTypeEnum.ReservedSeat:
                        carTypesText.Add("ReservedSeat");
                        break;
                    case CarTypeEnum.Compartment:
                        carTypesText.Add("Compartment");
                        break;
                    case CarTypeEnum.Luxury:
                        carTypesText.Add("Luxury");
                        break;
                }

            foreach (var train in root.Trains)
                if (train.LocalDepartureDateTime.ToString()!.Contains(inputNotificationTask.TimeFrom)) //Если в ответе содержится необходимая поездка
                    foreach (var carGroup in train.CarGroups)
                        if (!carGroup.HasPlacesForDisabledPersons) // Если место не для инвалидов                               
                            if (carTypesText.Contains(carGroup.CarType)) //Если это тот тип вагонов, которые выбрал пользователь
                                if (carGroup.TotalPlaceQuantity >= inputNotificationTask.NumberSeats)  //Если есть свободные места
                                {
                                    var key = carGroup.ServiceClassNameRu is not null ? carGroup.ServiceClassNameRu : carGroup.CarTypeName;
                                    //TODO: костыльевато, но по другому хз как. В пакете данных приходит только "СИД"
                                    if (key == "СИД") key = "Сидячий";
                                    var result = new SearchResult { CarType = key, TotalPlace = carGroup.TotalPlaceQuantity, Price = carGroup.MaxPrice };
                                    AddOrUpdateSearchResult(results, result);
                                }
            return results;
        }

        /// <summary>
        /// Добавляет SearchResult в коллекцию, а если он уже добавлен - обновляет поля
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="newItem"></param>
        private void AddOrUpdateSearchResult(HashSet<SearchResult> collection, SearchResult newItem)
        {
            if (collection.TryGetValue(newItem, out var existingItem))
                existingItem.TotalPlace = existingItem.TotalPlace + newItem.TotalPlace;
            else
                collection.Add(newItem);
        }

        private List<string> SupportingMethod(HashSet<SearchResult> currentRoutes)
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

        /// <inheritdoc/>
        public async Task<string?> GetLinkToBuyTicket(NotificationTask notificationTask)
        {
            var baseLink = "https://ticket.rzd.ru/searchresults/v/1";

            try
            {
                var departureStationNodeIdResponse = await _b2bClient.GetNodeIdStationAsync(notificationTask.DepartureStation);
                var arrivalStationNodeResponse = await _b2bClient.GetNodeIdStationAsync(notificationTask.ArrivalStation);

                var departureStationNodeId = GetNodeIdStation(departureStationNodeIdResponse);
                var arrivalStationNodeId = GetNodeIdStation(arrivalStationNodeResponse);

                if (departureStationNodeId == null || arrivalStationNodeId == null)
                    return null;

                var dateFromText = notificationTask.DateFrom.ToString("yyyy-MM-dd");

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
        private string GetNodeIdStation(string response)
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
                var textResponse = await _b2bClient.GetKsidAsync();

                //вытаскиваем из ответа строку со Ksid
                if (!textResponse.Contains("id")) throw new HttpRequestException($"Сервис Касперского вернул невалидный ответ:\n{textResponse}");
                Regex regex = new Regex("\"id\":\"(.*?)\"");
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
    }
}
