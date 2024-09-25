using Abp.Collections.Extensions;
using Abp.Domain.Uow;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using RailwayWizzard.B2B;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.UnitOfWork;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App.Services.B2B
{
    /// <inheritdoc/>
    public class B2BService : IB2BService
    {
        private readonly IB2BClient _b2bClient;
        private readonly IRailwayWizzardUnitOfWork _uow;

        /// <summary>
        /// Initialize bla bla bla
        /// </summary>
        /// <param name="b2bClient">B2B клиент для связи с РЖД.</param>
        public B2BService(IB2BClient b2bClient, IRailwayWizzardUnitOfWork uow)
        {
            _b2bClient = b2bClient;
            _uow = uow;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<string>> GetAvailableTimesAsync(ScheduleDto scheduleDto)
        {
            //подготовка данных
            scheduleDto.StationFrom = scheduleDto.StationFrom.ToUpper();
            scheduleDto.StationTo = scheduleDto.StationTo.ToUpper();

            var text = await _b2bClient.GetAvailableTimesAsync(scheduleDto);
            var availableTimes = ParseScheduleText(text, scheduleDto.Date);

            return availableTimes;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<StationInfo>> StationValidateAsync(string stationName)
        {
            //Ищем станцию по полному соответствию
            var station = await GetStationInfoAsync(stationName);
            if (station is not null) return new List<StationInfo> { station };

            //Ищем станцию по НЕполному соответствию
            var stations = await GetStationsInfo(stationName);

            return stations;
        }

        /// <summary>
        /// Парсит расписание в виде HTML и!
        /// </summary>
        /// <param name="textResponse"></param>
        /// <param name="dateFrom"></param>
        /// <returns></returns>
        private IReadOnlyCollection<string> ParseScheduleText(string textResponse, string dateFrom)
        {
            var availableTime = new List<string>();

            string moscowTodayTime = Common.GetMoscowDateTime.ToString("HH:mm");

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
                        // Если поезд на сегодня - проверяем допустимое для ввода время
                        if (dateFrom == DateTime.Now.ToString("dd.MM.yyyy"))
                        {
                            // Если время из расписания больше чем сейчас - добавляем его в список доступных для выбора
                            if (string.CompareOrdinal(timeRailwayStr, moscowTodayTime) > 0)
                                availableTime.Add(timeRailwayStr);
                        }
                        // Иначе просто отдаем расписание
                        else
                            availableTime.Add(timeRailwayStr);
                    }
                }
            }

            return availableTime;
        }

        // Переделал. Получаю ответ только из БД.
        private async Task<StationInfo?> GetStationInfoAsync(string stationName)
        {
            // Если есть в БД
            var stationInfo = await _uow.StationInfoRepository.FindByStationNameAsync(stationName);
            if (stationInfo is not null) { return stationInfo; }

            //Спрашиваем у АПИ
            var stations = await GetStations(stationName);
            // И у АПИ не нашли 
            if (stations.Count == 0) return null;

            ////Ищем среди полученных по АПИ нашу искомую станцию
            //var station = stations.SingleOrDefault(s => s.n.ToUpper() == stationName);
            //if (station is not null) { return new StationInfo { ExpressCode = station.c, StationName = station.n }; }

            // Снова смотрим есть ли в БД
            stationInfo = await _uow.StationInfoRepository.FindByStationNameAsync(stationName);
            if (stationInfo is not null) { return stationInfo; }

            return null;
        }

        /// <summary>
        /// Получение станций по НЕполному совпадению
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        private async Task<IReadOnlyCollection<StationInfo>> GetStationsInfo(string stationName)
        {
            //Ищем в БД
            var stationsInfo = await _uow.StationInfoRepository.ContainsByStationNameAsync(stationName);
            if (stationsInfo.Any()) return stationsInfo;

            //Ищем в данных полученных по АПИ
            var stations = await GetStations(stationName);
            if (stations.Count == 0) return new List<StationInfo>();
            return stations
                .Where(s => s.n.ToUpper().Contains(stationName.ToUpper()))
                .Select(s => new StationInfo { ExpressCode = s.c, StationName = s.n })
                .ToList();
        }

        private async Task<IReadOnlyCollection<StationFromJson>> GetStations(string inputStation)
        {
            var textResponse = await _b2bClient.GetStationsText(inputStation);
            if (textResponse.IsNullOrEmpty()) { return new List<StationFromJson>(); }
            var stations = DeserializeStationsText(textResponse);

            if (stations.Count > 0) await CreateStationsInfoAsync(stations);
            return stations;
        }
        private List<StationFromJson> DeserializeStationsText(string textResponse)
        {
            var stations = JsonConvert.DeserializeObject<List<StationFromJson>>(textResponse);
            if (stations!.Count == 0)
                throw new NullReferenceException(
                    $"Сервис РЖД при запросе списка свободных мест вернул ответ в котором нет поездок. Ответ:{textResponse}");

            return stations;
        }


        /// <summary>
        /// Добавляет в таблицу AppStationInfo новые записи
        /// </summary>
        /// <param name="rootStations"></param>
        /// <returns></returns>
        private async Task CreateStationsInfoAsync(IReadOnlyCollection<StationFromJson> rootStations)
        {
            var addedStationInfo = new List<StationInfo>();

            foreach (var rootStation in rootStations)
            {
                var anyStationInfo = await _uow.StationInfoRepository.AnyByExpressCodeAsync(rootStation.c);

                if (anyStationInfo is false)
                {
                    addedStationInfo.Add(new StationInfo
                    {
                        ExpressCode = rootStation.c,
                        StationName = rootStation.n,
                    });
                }
            }

            await _uow.StationInfoRepository.AddRangeStationInfoAsync(addedStationInfo);
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}
