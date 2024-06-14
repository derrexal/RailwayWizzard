using Newtonsoft.Json;
using RailwayWizzard.Robot.App;

namespace RailwayWizzard.App
{
    //TODO: вынести в отдельный сервис
    /// <summary>
    /// Вспомогательный воркер по проверке свободных окошек на необходимую мне услугу
    /// </summary>
    public class MosRuWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IBotApi _botApi;
        private readonly IRobot _robot;
        private const int timeInterval = 1000 * 60 * 3; //Интервал запуска (3 мин)
        private const string officeNumber = "363"; // Номер офиса чертого бутовского МФЦ
        public MosRuWorker(ILogger<MosRuWorker> logger, IBotApi botApi, IRobot robot)
        {
            _botApi = botApi;
            _logger = logger;
            _robot = robot;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(MosRuWorker)} running at: {DateTimeOffset.Now}");

                await DoWork();

                await Task.Delay(timeInterval, cancellationToken);
            }
        }

        private async Task DoWork()
        {
            try
            {
                var responseJsonInfoAboutFreeDay = await GetInfoAboutFreeDay();
                var resultInfoAboutFreeDay = DesirializeInfoAboutFreeDayResponse(responseJsonInfoAboutFreeDay);
                if (resultInfoAboutFreeDay is not null)
                    await _botApi.SendMessageForAdminAsync($"[{nameof(MosRuWorker)}] Появилось свободное место: {resultInfoAboutFreeDay}");

                var responseJsonInfoAboutFreeWindowFromDay = await GetInfoAboutFreeWindowFromDay();
                var resultInfoAboutFreeWindowFromDay = DesirializeInfoAboutFreeWindowFromDay(responseJsonInfoAboutFreeWindowFromDay);
                if(resultInfoAboutFreeWindowFromDay is not null)
                    await _botApi.SendMessageForAdminAsync($"[{nameof(MosRuWorker)}] Появилось свободное место: {resultInfoAboutFreeWindowFromDay}");
            }
            catch (Exception ex)
            {
                await _botApi.SendMessageForAdminAsync($"[{nameof(MosRuWorker)}] Возникла ошибка: {ex.Message}");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(MosRuWorker)} stopped at: {DateTimeOffset.Now}");
            await base.StopAsync(cancellationToken);
        }

        private async Task<string> GetInfoAboutFreeDay()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.mos.ru/pgu/common/ajax/NewBooking/loadNearestDateOffices");
            request.Headers.Add("accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            request.Headers.Add("Cookie", "PHPSESSID=0ffa97644ae224d5589f49a7cc6fe87b; session-cookie=17d8dabaef1c78b0355920bcbeb261f5fa48e78098cdbea704fe0c211cc6f77650f57aab07b9e6f04ee88071c4eff861");
            request.Content = new StringContent("method=nearestDateOffices&serviceId=115&person_type=1", null, "application/x-www-form-urlencoded");
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return responseJson;
        }

        private string? DesirializeInfoAboutFreeDayResponse(string responseJson)
        {
            var rootObject = JsonConvert.DeserializeObject<RootObject>(responseJson);
            if (rootObject is null || rootObject.Result is null) throw new ArgumentException("Сервис вернул невалидный ответ");
            var result = rootObject.Result.FirstOrDefault(r => r.Key == officeNumber).Value;
            return result;
        }

        private async Task<string> GetInfoAboutFreeWindowFromDay()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.mos.ru/pgu/common/ajax/NewBooking/getTimeSlots");
            request.Headers.Add("accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            request.Headers.Add("Cookie", "PHPSESSID=14e5dbdd203e1e9fe1c4350001eea43f; session-cookie=17d8e239e8b185a7355920bcbeb261f56945e0a6fe9267dc5be0a3e40746617f8cc985325da0f5a187bb911b16fca9a4");
            request.Content = new StringContent($"method=booking&serviceId=115&officeId={officeNumber}", null, "application/x-www-form-urlencoded");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return responseJson;
        }

        private string? DesirializeInfoAboutFreeWindowFromDay(string responseJson)
        {
            var rootObject = JsonConvert.DeserializeObject<ScheduleResponse>(responseJson);
            if (rootObject is null || rootObject.Schedule is null) throw new ArgumentException("Сервис вернул невалидный ответ");
            foreach (var schedule in rootObject.Schedule)
                foreach (var time in schedule.Value.Values)
                    if (time.Allow)
                        return schedule.Key;
            return null;
        }
    }

    public class RootObject
    {
        [JsonProperty("result")]
        public Dictionary<string, string> Result { get; set; }

        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("http_code")]
        public int HttpCode { get; set; }

        [JsonProperty("errorRequest")]
        public string ErrorRequest { get; set; }
    }

    public class ScheduleResponse
    {
        [JsonProperty("schedule")]
        public Dictionary<string, Dictionary<string, TimeSlot>> Schedule { get; set; }
    }

    public class TimeSlot
    {
        [JsonProperty("allow")]
        public bool Allow { get; set; }

        [JsonProperty("orig_startTime")]
        public DateTime OrigStartTime { get; set; }

        [JsonProperty("orig_endTime")]
        public DateTime OrigEndTime { get; set; }
    }
}