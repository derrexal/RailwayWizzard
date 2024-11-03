using Abp.Collections.Extensions;
using Abp.Extensions;
using RailwayWizzard.Core;

namespace RailwayWizzard.B2B
{
    /// <inheritdoc/>
    public class B2BClient : IB2BClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="B2BClient" class./>
        /// </summary>
        /// <param name="httpClientFactory"></param>
        public B2BClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private async Task<string> BaseHttpSender(HttpRequestMessage request)
        {
            using var client = _httpClientFactory.CreateClient();

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var textResponse = await response.Content.ReadAsStringAsync();

            //TODO: вынести везде в класс Ensure?
            //TODO: вынести в один exception все аналогичные места (пример на работе)
            //TODO: обработать эту ошибку и если она будет - не отдавать пользователю сссылку на сайт.
            if (textResponse.IsNullOrEmpty() || textResponse.IsNullOrWhiteSpace()) 
                throw new Exception($"Сервис РЖД при запросе {request.RequestUri} вернул пустой ответ");

            return textResponse;
        }

        /// <inheritdoc/>
        public async Task<string> GetNodeIdStationAsync(string stationName)
        {
            var requestUri = $"https://ticket.rzd.ru/api/v1/suggests?Query={stationName}&TransportType=bus,avia,rail,aeroexpress,suburban,boat&GroupResults=true&RailwaySortPriority=true&SynonymOn=1";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Accept-Language", "ru,en;q=0.9");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 YaBrowser/24.7.0.0 Safari/537.36");

            return await BaseHttpSender(request);
        }

        /// <inheritdoc/>
        public async Task<string> GetTrainInformationByParametersAsync(NotificationTask inputNotificationTask, string ksid)
        {
            string url = $"https://ticket.rzd.ru/apib2b/p/Railway/V1/Search/TrainPricing?service_provider=B2B_RZD&bs={ksid}";

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
                "\"GetTrainsFromSchedule\":true}"
                , null
                , "application/json");

            return await BaseHttpSender(request);
        }

        /// <inheritdoc/>
        public async Task<string> GetStationsText(string inputStation)
        {
            var uriInputStation = Uri.EscapeDataString(inputStation);

            string url = $"https://pass.rzd.ru/suggester/?stationNamePart={uriInputStation}&lang=ru";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Host", "pass.rzd.ru");

            return await BaseHttpSender(request);
        }

        /// <inheritdoc/>
        public async Task<string> GetKsidAsync()
        {
            string url = "https://w-22900.fp.kaspersky-labs.com/oxwdsq?cid=22900";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            return await BaseHttpSender(request);
        }
    }
}
