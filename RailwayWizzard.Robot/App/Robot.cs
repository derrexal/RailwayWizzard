using System.Globalization;
using System.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RailwayWizzard.Robot.Core;
using RzdHack.Robot.Core;


namespace RzdHack.Robot.App
{
    public class Robot
    {
        private const string _baseUrl = "php_service:8088/routes/";
        private readonly ILogger _logger;

        public Robot(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Получает информацию о свободных местах по заданным параметрам 
        /// </summary>
        /// <task name="param"></task>
        /// <returns></returns>
        public async Task<List<string>> GetTicket(NotificationTask task)
        {
            var url = SetUrlFromGetTicket(task);
            HttpClient client = new HttpClient();
            string? jsonResponse = null;
            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                using HttpResponseMessage response = await client.SendAsync(request);
                jsonResponse = await response.Content.ReadAsStringAsync();
                var roots = JsonConvert.DeserializeObject<List<Root>>(jsonResponse);
                if (roots != null)
                    return GetCurrentRouteFromResponse(roots, task.TimeFrom);
                throw new HttpRequestException("Сервис php не вернул ответ");
            }

            catch (JsonReaderException je)
            {
                _logger.LogError($"Не удалось распарсить ответ в JSON\n\n{jsonResponse}");
                throw;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Не доступен сервис php\n{e}");
                throw;
            }
        }

        /// <summary>
        /// Формирует строку запроса для php-сервиса по заданным параметрам
        /// </summary>
        /// <task name="task"></task>
        /// <returns></returns>
        private string SetUrlFromGetTicket(NotificationTask task)
        {
            var builder = new UriBuilder(_baseUrl);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["layer_id"] = "5827";
            query["dir"] = "0";
            query["tfl"] = "3";
            query["checkSeats"] = "1";
            query["code0"] = task.DepartureStationCode.ToString();
            query["code1"] = task.ArrivalStationCode.ToString();
            query["dt0"] = task.DateFrom.ToString("dd.MM.yyy", CultureInfo.InvariantCulture);
            builder.Query = query.ToString();
            return builder.ToString();
        }

        /// <summary>
        /// Парсит ответ и возвращает информацию о запрашиваемой поездке
        /// </summary>
        /// <returns></returns>
        private List<string> GetCurrentRouteFromResponse(List<Root> roots, string departureTime)
        {
            List<string> result=new();
            foreach (var route in roots)
                if (route.time0 == departureTime && route.cars != null)
                    foreach (var car in route.cars)
                        if (!car.typeLoc.Contains("инвалид")) //выходит, что эта проверка сейчас бесполезна...
                            if (car.freeSeats != null)
                                result.Add($"Класс обслуживания: <strong>{car.typeLoc}</strong> \nСвободных мест: <strong>{car.freeSeats}</strong>\n");
            return result;
        }
        
        #region Old

        //private const string _getTicketUrl = "https://pass.rzd.ru/timetable/public/?layer_id=5827&dir=0&tfl=3&checkSeats=1&code0=2004000&code1=2000000&dt0=27.01.2024";
        //private const string _baseGetTicketUrl = "https://pass.rzd.ru/timetable/public/ru?layer_id=5827";


        //public static async Task<string> GetTicketOld()
        //{
        //    HttpClient client = new HttpClient();
        //    using HttpRequestMessage firstRequest = new HttpRequestMessage(HttpMethod.Get, _getTicketUrl);
        //    using HttpResponseMessage firstResponse = await client.SendAsync(firstRequest);
        //    var firstContent = await firstResponse.Content.ReadAsStringAsync();

        //    Console.WriteLine(firstContent);
        //    Console.WriteLine(firstResponse.StatusCode);
        //    var rid = JObject.Parse(firstContent)["RID"]?.ToString();



        //    firstResponse.Headers.TryGetValues("Set-Cookie", out var setCookie);

        //    //Thread.Sleep(61000);//61c

        //    var url = _baseGetTicketUrl + "&rid=" + rid;
        //    using HttpRequestMessage secondRequest = new HttpRequestMessage(HttpMethod.Get, url);
        //    //secondRequest.Content = new StringContent($"rid={rid}", Encoding.UTF8, "application/x-www-form-urlencoded");
        //    //secondRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        //    secondRequest.Headers.Add("Cookie", setCookie!);
        //    secondRequest.Headers.Add("ContentType", "application/x-www-form-urlencoded"!);
        //    using HttpResponseMessage secondResponse = await client.SendAsync(secondRequest);
        //    var secondContent = await secondResponse.Content.ReadAsStringAsync();

        //    Console.WriteLine(secondContent);
        //    Console.WriteLine(secondResponse.StatusCode);
        //    return secondContent;
        //}


        #endregion

    }
}