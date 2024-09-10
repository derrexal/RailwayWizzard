using Abp.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RailwayWizzard.Core;
using RailwayWizzard.Robot.Core;
using System.Text.RegularExpressions;

namespace RailwayWizzard.Robot.App
{
    /// <summary>
    /// Класс для работы с АПИ РЖД
    /// </summary>
    public class RobotBigBrother: IRobot
    {
        private readonly ILogger<RobotBigBrother> _logger;
        private readonly IHttpClientFactory _httpClientFactory;


        public RobotBigBrother(ILogger<RobotBigBrother> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Получение информации о свободных мест в запрашиваемом рейсе
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        public async Task<List<string>> GetFreeSeatsOnTheTrain(NotificationTask inputNotificationTask)
        {
            const int retryCount = 3;
            
            var freeSeats = await GetFreeSeatsOnTheTrainHelper(inputNotificationTask);

            if(freeSeats.Count > 0) return freeSeats;

            for(int i = 0; i <= retryCount; i++)
            {
                if (freeSeats.Count == 0)
                {
                    await Task.Delay(60000);

                    freeSeats = await GetFreeSeatsOnTheTrainHelper(inputNotificationTask);
                }
                else
                    break;
            }

            return freeSeats;
        }

        public async Task<List<string>> GetFreeSeatsOnTheTrainHelper(NotificationTask inputNotificationTask)
        {
            var textResponse = await GetTrainInformationByParameters(inputNotificationTask);
            //TODO: нужно смапить в DTO чтобы этот огромный объект не таскать по памяти
            RootBigBrother? myDeserializedClass = JsonConvert.DeserializeObject<RootBigBrother>(textResponse);

            if (myDeserializedClass == null)
                throw new NullReferenceException($"Сервис РЖД при запросе списка свободных мест вернул не стандартный ответ. Ответ:{textResponse}");
            if (myDeserializedClass.Trains.Count == 0)
            {
                _logger.LogError($"Сервис РЖД при запросе списка свободных мест вернул ответ в котором нет доступных поездок. Ответ:{textResponse}");
                return new List<string>();
            }

            //вытаскиваем свободные места по запрашиваемому рейсу
            var currentRoute = GetCurrentRouteFromResponse(myDeserializedClass, inputNotificationTask);
            if (currentRoute.Count == 0) return new List<string>();

            //Формируем текстовый ответ пользователю
            var result = SupportingMethod(currentRoute);
            return result;
        }
        /// <summary>
        /// Получение информации о рейсах по запрашиваемым параметрам
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        private async Task<string> GetTrainInformationByParameters(NotificationTask inputNotificationTask)
        {
            //адрес сервиса получения рейсов по заданной дате
            string url = "https://ticket.rzd.ru/apib2b/p/Railway/V1/Search/TrainPricing?service_provider=B2B_RZD&bs=";

            //Дополняем строку запроса
            string ksid = await GetKsidForGetTicket();
            url = url + ksid;

            //Устаналиваем необходимые headers и body
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Origin", "https://ticket.rzd.ru");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            request.Headers.Add("Cookie", " LANG_SITE=ru; " + $"oxxfgh={ksid}");
            request.Content = new StringContent(
                "{\"Origin\":\"" + inputNotificationTask.DepartureStationCode + "\"," +
                "\"Destination\":\"" + inputNotificationTask.ArrivalStationCode + "\"," +
                "\"DepartureDate\":\"" + inputNotificationTask.DateFrom.ToString("yyyy-MM-ddT00:00:00") + "\"," +
                "\"TimeFrom\":0," +
                "\"TimeTo\":24," +
                "\"CarGrouping\":\"DontGroup\"," +
                "\"GetByLocalTime\":true," +
                "\"SpecialPlacesDemand\":\"StandardPlacesAndForDisabledPersons\"," +
                "\"CarIssuingType\":\"All\"," +
                "\"GetTrainsFromSchedule\":false}"
                , null, "application/json");

            //отправляем запрос
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var textResponse = await response.Content.ReadAsStringAsync();
            if (textResponse.IsNullOrEmpty()) { throw new Exception("Сервис РЖД при запросе списка свободных мест вернул пустой ответ"); }
            return textResponse;
        }

        /// <summary>
        /// Получение информации о свободных местах в запрашиваемый день по запрашиваемому рейсу
        /// </summary>
        /// <param name="root"></param>
        /// <param name="departureTime"></param>
        /// <returns></returns>
        private HashSet<SearchResult> GetCurrentRouteFromResponse(RootBigBrother root, NotificationTask inputNotificationTask)
        {
            HashSet<SearchResult> results = new ();

            //todo: костыль, но как по другому?
            //наполяем список типов вагонов словами, чтобы было с чем сравнивать
            List<string> carTypesText = new List<string>();
            foreach(var carType in inputNotificationTask.CarTypes)
                switch(carType)
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
                            if(carTypesText.Contains(carGroup.CarType)) //Если это тот тип вагонов, которые выбрал пользователь
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

        //TODO: Отправлять в бот только словарь свободных мест а эту логику переложить на бота! Ни к чему гонять такие объемы данных по сети
        private List<string> SupportingMethod(HashSet<SearchResult> currentRoutes)
        {
            List<string> result = new();
            foreach(var route in currentRoutes)
                result.Add($"Класс обслуживания: <strong>{route.CarType}</strong>\nСвободных мест: <strong>{route.TotalPlace}</strong>\nЦена: <strong> {route.Price}</strong>\n");
            return result;
        }

        /// <summary>
        /// Получение значения "oxwdsq"(Ksid) для дальнейших запросов
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetKsidForGetTicket()
        {
            string url = "https://w-22900.fp.kaspersky-labs.com/oxwdsq?cid=22900";
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var client = _httpClientFactory.CreateClient();
                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var textResponse = await response.Content.ReadAsStringAsync();
                
                //вытаскиваем из ответа строку со Ksid
                if (!textResponse.Contains("id")) throw new HttpRequestException($"Сервис Касперского вернул невалидный ответ:\n{textResponse}");
                Regex regex = new Regex("\"id\":\"(.*?)\"");
                var res = regex.Match(textResponse).ToString();
                
                //Из всей строки получаем только значение
                var keyValuePairs = res.Split(':').ToList();
                var result = keyValuePairs.LastOrDefault();
                if (result is null) throw new HttpRequestException($"Не удалось распарсить ответ от Касперского:\n{textResponse}");
                
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
    }
}
