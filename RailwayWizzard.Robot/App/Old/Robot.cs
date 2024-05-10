#region Устаревшая версия робота, который использовал внешний сервис для доступа к РЖД АПИ


//using System.Globalization;
//using System.Web;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using RailwayWizzard.Core;


//namespace RailwayWizzard.Robot.App.Old
//{
//    public class Robot
//    {
//        private const string _baseUrl = "php_service:8088/";
//        private readonly ILogger _logger;

//        public Robot(ILogger logger)
//        {
//            _logger = logger;
//        }

//        /// <summary>
//        /// Получает информацию о свободных местах по заданным параметрам 
//        /// </summary>
//        /// <task name="param"></task>
//        /// <returns></returns>
//        public async Task<List<string>> GetTicket(NotificationTask inputNotificationTask)
//        {
//            var url = SetUrlFromGetTicket(inputNotificationTask);
//            HttpClient client = new HttpClient();
//            string? jsonResponse = null;
//            try
//            {
//                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
//                using HttpResponseMessage response = await client.SendAsync(request);
//                jsonResponse = await response.Content.ReadAsStringAsync();
//                var roots = JsonConvert.DeserializeObject<List<Root>>(jsonResponse);
//                if (roots != null)
//                    return GetCurrentRouteFromResponse(roots, inputNotificationTask.TimeFrom);
//                throw new HttpRequestException("Сервис php не вернул ответ");
//            }

//            catch (JsonReaderException je)
//            {
//                _logger.LogError($"Не удалось распарсить ответ в JSON.Далее последует стек ошибок php сервиса\n\n{jsonResponse}");
//                throw;
//            }
//            catch (HttpRequestException e)
//            {
//                _logger.LogError($"Не доступен сервис php\n{e}");
//                throw;
//            }
//        }

//        /// <summary>
//        /// Формирует строку запроса для php-сервиса по заданным параметрам
//        /// </summary>
//        /// <task name="task"></task>
//        /// <returns></returns>
//        private string SetUrlFromGetTicket(NotificationTask task)
//        {
//            var builder = new UriBuilder(_baseUrl + "routes/");
//            var query = HttpUtility.ParseQueryString(builder.Query);
//            query["layer_id"] = "5827";
//            query["dir"] = "0";
//            query["tfl"] = "3";
//            query["checkSeats"] = "1";
//            query["code0"] = task.DepartureStationCode.ToString();
//            query["code1"] = task.ArrivalStationCode.ToString();
//            query["dt0"] = task.DateFrom.ToString("dd.MM.yyy", CultureInfo.InvariantCulture);
//            builder.Query = query.ToString();
//            return builder.ToString();
//        }


//        //TODO: оптимизировать: возвращать не список строк, а словарь {car.typeLoc} {car.freeSeats}, а выше уже распарсить в сообщение динамически. Или вообще распарсить на клиенте (в коде бота)
//        /// <summary>
//        /// Парсит ответ и возвращает информацию о запрашиваемой поездке
//        /// </summary>
//        /// <returns></returns>
//        private List<string> GetCurrentRouteFromResponse(List<Root> roots, string departureTime)
//        {
//            List<string> result = new();
//            foreach (var route in roots)
//                //Если в ответе содержится необходимая поездка
//                if (route.time0 == departureTime && route.cars != null)
//                    foreach (var car in route.cars)
//                        //Если есть свободные места
//                        if (car.freeSeats != null)
//                        {
//                            //если место не для инвалидов
//                            if (car.disabledPerson == null || car.disabledPerson == false)
//                                //Оставил на случай, если понадобится оповещать еще и о местах для инвалидов
//                                //car.typeLoc = car.typeLoc + " (для инвалидов)";
//                                result.Add($"Класс обслуживания: <strong>{car.typeLoc}</strong> \nСвободных мест: <strong>{car.freeSeats}</strong>\n");
//                        }
//            return result;
//        }
//    }
//}

#endregion