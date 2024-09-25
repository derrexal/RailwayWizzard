﻿using Abp.Collections.Extensions;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.UnitOfWork;

namespace RailwayWizzard.B2B
{
    public class B2BClient: IB2BClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRailwayWizzardUnitOfWork _uow;

        public B2BClient(IHttpClientFactory httpClientFactory, IRailwayWizzardUnitOfWork uow)
        {
            _httpClientFactory = httpClientFactory;
            _uow = uow;
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
            //request.Headers.Add("Cookie", "session-cookie=17f445bdc336e92bd289545f80267f93d8216047fdf2afb328ffe7a0822a63eb358b2f62bda51aa652544e9f0eab2dff");
            
            //TODO: Вынести в условный baseHttpSender?
            using var client = _httpClientFactory.CreateClient();
            
            using var response = await client.SendAsync(request);
            
            response.EnsureSuccessStatusCode();
            
            var textResponse = await response.Content.ReadAsStringAsync();
            
            //TODO: вынести везде в класс Ensure?
            //TODO: вынести в один exception все аналогичные места (пример на работе)
            //TODO: обработать эту ошибку и если она будет - не отдавать пользователю сссылку на сайт.
            if (textResponse.IsNullOrEmpty()) { throw new Exception("Сервис РЖД при запросе станции по имени вернул пустой ответ"); }
            
            return textResponse;
        }

        /// <inheritdoc/>
        public async Task<string> GetTrainInformationByParametersAsync(NotificationTask inputNotificationTask, string ksid)
        {
            //адрес сервиса получения рейсов по заданной дате
            string url = $"https://ticket.rzd.ru/apib2b/p/Railway/V1/Search/TrainPricing?service_provider=B2B_RZD&bs={ksid}";

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

        /// <inheritdoc/>
        public async Task<string> GetStationsText(string inputStation)
        {
            var uriInputStation = Uri.EscapeDataString(inputStation);
            string url = $"https://pass.rzd.ru/suggester/?stationNamePart={uriInputStation}&lang=ru";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Host", "pass.rzd.ru");
            
            using var client = _httpClientFactory.CreateClient();
            
            using var response = await client.SendAsync(request);
            
            response.EnsureSuccessStatusCode();
            
            var textResponse = await response.Content.ReadAsStringAsync();
            
            if (textResponse.IsNullOrEmpty()) { throw new Exception("Сервис РЖД при запросе списка свободных мест вернул пустой ответ"); }
            
            return textResponse;
;       }

        /// <inheritdoc/>
        public async Task<string> GetAvailableTimesAsync(ScheduleDto scheduleDto)
        {
            // тут было обращение и к БД и к АПИ.
            // Решил что смысла ходить к АПИ нет, т.к. на моменте когда запрашивается расписания станция уже точно должна быть в БД
            var stationFrom = await _uow.StationInfoRepository.FindByStationNameAsync(scheduleDto.StationFrom);
            var stationTo = await _uow.StationInfoRepository.FindByStationNameAsync(scheduleDto.StationTo);
            
            if (stationTo is null || stationFrom is null) throw new ArgumentException($"Не найден ExpressCode по станциям {scheduleDto.StationFrom},{scheduleDto.StationTo}. Вероятно в них допущена ошибка");

            string url = "https://pass.rzd.ru/basic-schedule/public/ru?STRUCTURE_ID=5249&layer_id=5526&refererLayerId=5526&" +
                         $"st_from={stationFrom.ExpressCode}&st_to={stationTo.ExpressCode}&st_from_name={scheduleDto.StationFrom}&st_to_name={scheduleDto.StationTo}&day={scheduleDto.Date}";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Host", "pass.rzd.ru");

            using var client = _httpClientFactory.CreateClient();
            
            using var response = await client.SendAsync(request);
            
            response.EnsureSuccessStatusCode();
            
            var textResponse = await response.Content.ReadAsStringAsync();
            
            if (textResponse.IsNullOrEmpty()) throw new Exception("Сервис РЖД при запросе списка свободных мест вернул пустой ответ");
            
            return textResponse;
        }

        /// <inheritdoc/>
        public async Task<string> GetKsidAsync()
        {
            string url = "https://w-22900.fp.kaspersky-labs.com/oxwdsq?cid=22900";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
                
            using var client = _httpClientFactory.CreateClient();
                
            using var response = await client.SendAsync(request);
                
            response.EnsureSuccessStatusCode();

            var textResponse = await response.Content.ReadAsStringAsync();

            if (textResponse.IsNullOrEmpty()) throw new Exception("Сервис РЖД при запросе списка свободных мест вернул пустой ответ");
                
            return textResponse;
        }
    }
}