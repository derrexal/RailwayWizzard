using RailwayWizzard.Core;

namespace RailwayWizzard.B2B
{
    /// <summary>
    /// Сервис для общения с РЖД.
    /// </summary>
    public interface IB2BClient
    {
        /// <summary>
        /// Возвращает NodeId станции.
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns>Код найденной станции</returns>
        public Task<string> GetNodeIdStationAsync(string stationName);

        /// <summary>
        /// Получение информации о рейсах по запрашиваемым параметрам
        /// </summary>
        /// <param name="inputNotificationTask">Задача</param>
        /// <param name="ksid">Токен стороннего сервиса</param>
        /// <param name="getTrainsFromSchedule">Необходимо ли возвращать поезда на которые билетов нет или не продаются.</param>
        /// <returns></returns>
        public Task<string> GetTrainInformationByParametersAsync(NotificationTask inputNotificationTask, string ksid, bool getTrainsFromSchedule=true);

        /// <summary>
        /// Возвращает информацию о найденных станциях.
        /// </summary>
        /// <param name="inputStation"></param>
        /// <returns></returns>
        public Task<string> GetStationsText(string inputStation);

        /// <summary>
        /// Получение значения "oxwdsq"(Ksid) для дальнейших запросов
        /// </summary>
        /// <returns>Значение Ksid</returns>
        public Task<string> GetKsidAsync();
    }
}