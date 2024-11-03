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
        /// <returns></returns>
        public Task<string> GetTrainInformationByParametersAsync(NotificationTask inputNotificationTask, string ksid);

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