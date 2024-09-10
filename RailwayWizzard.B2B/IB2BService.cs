using RailwayWizzard.Core;

namespace RailwayWizzard.B2B
{
    /// <summary>
    /// B2B сервис.
    /// </summary>
    public interface IB2BService
    {
        /// <summary>
        /// Возвращает актуальное расписание для указанного рейса.
        /// </summary>
        /// <param name="scheduleDto">Рейс.</param>
        /// <returns>Актуальное расписание.</returns>
        public Task<IList<string>> GetAvailableTimes(ScheduleDto scheduleDto);

        /// <summary>
        /// Проверяет существует ли такая станция по полному или неполному соответствию.
        /// </summary>
        /// <param name="stationName">Станция введенная пользователем.</param>
        /// <returns>Список станций.</returns>
        public Task<List<StationInfo>> StationValidate(string stationName);
    }
}