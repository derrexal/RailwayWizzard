﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RailwayWizzard.Core;
using RailwayWizzard.Robot.Core;
using System.Text.RegularExpressions;

namespace RailwayWizzard.Robot.App
{
    //TODO: вынести в отдельный сервис

    /// <summary>
    /// Класс для работы с АПИ РЖД
    /// </summary>
    public class RobotBigBrother
    {
        private readonly ILogger _logger;

        public RobotBigBrother(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<string>> GetTicket(NotificationTask inputNotificationTask)
        {
            //адрес сервиса получения рейсов по заданной дате
            string url = "https://ticket.rzd.ru/apib2b/p/Railway/V1/Search/TrainPricing?service_provider=B2B_RZD&bs="; 

            try
            {
                //Дополняем строку запроса
                string ksid = await GetKsidForGetTicket();
                url = url + ksid;

                //Устаналиваем необходимые headers и body
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Accept", "application/json, text/plain, */*");
                request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9");
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Origin", "https://ticket.rzd.ru");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
                request.Headers.Add("Cookie"," LANG_SITE=ru; " + $"oxxfgh={ksid}");
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
                var client = new HttpClient();
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var textResponse = await response.Content.ReadAsStringAsync();
                if (textResponse == null || textResponse == "") { throw new Exception("Сервис РЖД при запросе списка свободных мест вернул пустой ответ"); }
                //TODO: нужно смапить в DTO чтобы этот огромный объект не таскать по памяти
                RootBigBrother myDeserializedClass = JsonConvert.DeserializeObject<RootBigBrother>(textResponse);
                if(myDeserializedClass == null) { throw new Exception("Сервис РЖД при запросе списка свободных мест вернул пустой ответ"); }
                if(myDeserializedClass.Trains.Count == 0) { throw new Exception("Сервис РЖД при запросе списка свободных мест вернул ответ в котором нет поездок"); }
                //вытаскиваем свободные места по запрашиваемому рейсу
                var currentRoute = GetCurrentRouteFromResponse(myDeserializedClass, inputNotificationTask.TimeFrom);
                var result = SupportingMethod(currentRoute);
                return result;
            }
            catch { throw; }
        }

        /// <summary>
        /// Из ответа по рейсам в запрашиваемый день вытаскивает свободные места по запрашиваемому рейсу
        /// </summary>
        /// <param name="root"></param>
        /// <param name="departureTime"></param>
        /// <returns></returns>
        private Dictionary<string,int> GetCurrentRouteFromResponse(RootBigBrother root, string departureTime)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            try
            {
                foreach (var train in root.Trains)
                    if (train.LocalDepartureDateTime.ToString()!.Contains(departureTime)) //Если в ответе содержится необходимая поездка
                        foreach (var carGroup in train.CarGroups)
                            if (!carGroup.HasPlacesForDisabledPersons) // Если место не для инвалидов
                                if (carGroup.TotalPlaceQuantity > 0)  //Если есть свободные места
                                {
                                    var key = carGroup.ServiceClassNameRu is not null ? carGroup.ServiceClassNameRu : carGroup.CarTypeName;
                                    //TODO: костыльевато, но по другому хз как. В пакете данных приходит только "СИД"
                                    if (key == "СИД") key = "Сидячий";
                                    if (!result.TryGetValue(key, out int value)) // если элемента нет в коллекции
                                        result.Add(key, carGroup.TotalPlaceQuantity);
                                    else
                                        result[key] = value + carGroup.TotalPlaceQuantity;
                                }
            }
            catch { throw; }
            return result;
        }

        //TODO: Отправлять в бот только словарь свободных мест а эту логику переложить на бота! Ни к чему гонять такие объемы данных по сети
        private List<string> SupportingMethod(Dictionary<string, int> currentRoutes)
        {
            List<string> result = new();
            foreach(var route in currentRoutes)
                result.Add($"Класс обслуживания: <strong>{route.Key}</strong> \nСвободных мест: <strong>{route.Value}</strong>\n");
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
                HttpClient client = new HttpClient();
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                using HttpResponseMessage response = await client.SendAsync(request);
                var textResponse = await response.Content.ReadAsStringAsync();
                
                //вытаскиваем из ответа строку со Ksid
                if (!textResponse.Contains("id")) throw new HttpRequestException($"Сервис Касперского вернул невалидный ответ:\n{textResponse}");
                Regex regex = new Regex("\"id\":\"(.*?)\"");
                var res = regex.Match(textResponse).ToString();
                
                //Из всей строки получаем только значение
                var keyValuePairs = res.Split(':').ToList();
                var result = keyValuePairs.LastOrDefault();
                if (result == null) throw new HttpRequestException($"Не удалось распарсить ответ от Касперского:\n{textResponse}");
                
                result = result.Remove(result.Length - 1);
                result = result.Remove(0, 1);
                return result;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Не доступен сервис Касперского для получения токена\n{e}");
                throw;
            }
            catch { throw; }
        }
    }
}