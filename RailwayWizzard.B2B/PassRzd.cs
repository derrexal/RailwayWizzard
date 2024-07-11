using Abp.Collections.Extensions;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;


namespace RailwayWizzard.B2B
{
    public class PassRzd: IPassRzd
    {
        private readonly RailwayWizzardAppContext _context;

        public PassRzd(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        public async Task<List<RootStations>> GetStations(string inputStation)
        {
            var textResponse = await GetStationsText(inputStation);
            var stations = DeserializeStationsText(textResponse);
            if (stations.Count>0) await CreateStationsInfoAsync(stations);
            return stations;
        }

        public async Task<IList<string>> GetAvailableTimes(ScheduleDto scheduleDto)
        {
            var text = await GetScheduleText(scheduleDto);
            var availableTimes = ParseScheduleText(text, scheduleDto.Date);
            return availableTimes;
        }

        private async Task<string> GetStationsText(string inputStation)
        {
            var uriInputStation = Uri.EscapeDataString(inputStation);
            string url = $"https://pass.rzd.ru/suggester/?stationNamePart={uriInputStation}&lang=ru";
            HttpClient client = new HttpClient();
            using HttpRequestMessage request = new (HttpMethod.Get, url);
            request.Headers.Add("Host", "pass.rzd.ru");
            using HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var textResponse = await response.Content.ReadAsStringAsync();
            if (textResponse.IsNullOrEmpty()) { throw new Exception("Сервис РЖД при запросе информации о станции вернул пустой ответ"); }
            return textResponse;
;       }

        private List<RootStations> DeserializeStationsText(string textResponse)
        {
            var stations = JsonConvert.DeserializeObject<List<RootStations>>(textResponse);
            if (stations!.Count == 0)
                throw new NullReferenceException(
                    $"Сервис РЖД при запросе списка свободных мест вернул ответ в котором нет поездок. Ответ:{textResponse}");

            return stations;
        }

        /// <summary>
        /// Получение расписания в формате HTML разметки
        /// </summary>
        /// <param name="scheduleDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> GetScheduleText(ScheduleDto scheduleDto)
        {
            var stationFrom = await GetStationInfo(scheduleDto.StationFrom);
            var stationTo = await GetStationInfo(scheduleDto.StationTo);
            if (stationTo is null || stationFrom is null) throw new ArgumentException($"Не найден ExpressCode по станциям {scheduleDto.StationFrom},{scheduleDto.StationTo}. Вероятно в них допущена ошибка");

            HttpClient client = new HttpClient();
            string url = "https://pass.rzd.ru/basic-schedule/public/ru?STRUCTURE_ID=5249&layer_id=5526&refererLayerId=5526&" +
                         $"st_from={stationFrom.ExpressCode}&st_to={stationTo.ExpressCode}&st_from_name={scheduleDto.StationFrom}&st_to_name={scheduleDto.StationTo}&day={scheduleDto.Date}";
            using HttpRequestMessage request = new(HttpMethod.Get, url);
            request.Headers.Add("Host", "pass.rzd.ru");
            using HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var textResponse = await response.Content.ReadAsStringAsync();
            if (textResponse.IsNullOrEmpty()) throw new Exception("Сервис РЖД при запросе списка свободных мест вернул пустой ответ");
            return textResponse;
        }

        /// <summary>
        /// Парсит расписание в виде HTML и!
        /// </summary>
        /// <param name="textResponse"></param>
        /// <param name="dateFrom"></param>
        /// <returns></returns>
        private IList<string> ParseScheduleText(string textResponse, string dateFrom)
        {
            var availableTime = new List<string>();
            string todayTime = DateTime.Now.ToString("HH:mm");
            IHtmlDocument document = new HtmlParser().ParseDocument(textResponse);
            var table = document.QuerySelector("table.basicSched_trainsInfo_table");
            if (table != null)
            {
                var rows = table.QuerySelectorAll("tr");
                foreach (var row in rows)
                {
                    var cells = row.QuerySelectorAll("td");
                    if (cells.Length > 1)
                    {
                        var timeRailwayStr = cells[1].TextContent;
                        if (dateFrom == DateTime.Now.ToString("dd.MM.yyyy"))
                        {
                            // Если время из расписания больше чем сейчас - добавляем его в список доступных для выбора
                            if (String.CompareOrdinal(timeRailwayStr, todayTime) > 0)
                                availableTime.Add(timeRailwayStr);
                        }
                        else
                            availableTime.Add(timeRailwayStr);
                    }
                }
            }

            return availableTime;
        }

        /// <summary>
        /// Получение станции по полному совпадению
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        public async Task<StationInfo?> GetStationInfo(string stationName)
        {
            //Если есть в БД
            var stationInfo = await _context.StationInfo.SingleOrDefaultAsync(s => s.StationName == stationName);
            if(stationInfo is not null) { return stationInfo; }

            //Спрашиваем у АПИ
            var stations = await GetStations(stationName);
            if(stations.Count==0) return null;

            //Ищем среди полученных по АПИ нашу искомую станцию
            var station = stations.SingleOrDefault(s => s.n == stationName);
            if(station is not null) { return new StationInfo{ExpressCode = station.c, StationName = station.n}; }

            return null;
        }

        /// <summary>
        /// Получение станций по НЕполному совпадению
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        private async Task<List<StationInfo>> GetStationsInfo(string stationName)
        {
            //Ищем в БД
            var stationsInfo = await _context.StationInfo.Where(s => s.StationName.Contains(stationName)).ToListAsync();
            if (stationsInfo.Any()) return stationsInfo;

            //Ищем в данных полученных по АПИ
            var stations = await GetStations(stationName);
            if (stations.Count == 0) return new List<StationInfo>();
            return stations
                .Where(s => s.n.Contains(stationName))
                .Select(s => new StationInfo { ExpressCode = s.c, StationName = s.n })
                .ToList();
        }

        public async Task<List<StationInfo>> StationValidate(string stationName)
        {
            //Ищем станцию по полному соответствию
            var station = await GetStationInfo(stationName);
            if (station is not null) return new List<StationInfo> { station}; 

            //Ищем станцию по НЕполному соответствию
            var stations = await GetStationsInfo(stationName);
            return stations;
        }

        /// <summary>
        /// Добавляет в таблицу AppStationInfo новые записи
        /// </summary>
        /// <param name="rootStations"></param>
        /// <returns></returns>
        private async Task CreateStationsInfoAsync(List<RootStations> rootStations)
        {
            foreach (var rootStation in rootStations)
                if(await _context.StationInfo.AnyAsync(s=>s.StationName==rootStation.n))
                    await _context.StationInfo.AddAsync(new StationInfo
                        {
                            ExpressCode = rootStation.c,
                            StationName = rootStation.n,
                        });
            await _context.SaveChangesAsync();
        }
    }
}
