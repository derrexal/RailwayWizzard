using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RzdHack.Robot.Core;


namespace RzdHack.Robot.App
{
    public class Robot
    {
        private const string _baseUrl = "http://localhost:8000/routes/";

        /// <summary>
        /// Получает информацию о свободных местах по заданным параметрам 
        /// </summary>
        /// <task name="param"></task>
        /// <returns></returns>
        public async Task<string> GetTicket(NotificationTask task)
        {
            var url = SetUrlFromGetTicket(task);
            HttpClient client = new HttpClient();
            
            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                using HttpResponseMessage response = await client.SendAsync(request);
                var сontent = await response.Content.ReadAsStringAsync();
                JArray obj = JsonConvert.DeserializeObject<JArray>(сontent);

                var result = GetCurrentRouteFromResponse(obj, task.TimeFrom);
                return result;
            }

            catch (HttpRequestException e)
            {
                Console.WriteLine($"Не доступен сервис php \n {e}");
                throw;
            }
        }

        /// <summary>
        /// Формирует URL по заданным параметрам
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
            query["dt0"] = task.DateFrom.ToShortDateString(); //TODO:Возможно тут он ждет другую дату (без времени...)
            builder.Query = query.ToString();
            return builder.ToString();
        }

        /// <summary>
        /// Парсит ответ и возвращает нужную поездку
        /// </summary>
        /// <returns></returns>
        private string? GetCurrentRouteFromResponse(JArray data, string departureTime)
        {
            string? result = null;
            foreach (var route in data)
            {
                foreach (var field in route)
                {
                    //нужно подумать и сделать по нормальному
                    var fieldString = field.ToString();
                    if (fieldString.Contains("time0") && fieldString.Contains(departureTime))
                        //Console.WriteLine(route);//TODO:если это поле - время отправления, если это время отправления равно тому которое мы ищем и если там есть свободные места - забираем
                        return route.ToString();
                }
            }

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