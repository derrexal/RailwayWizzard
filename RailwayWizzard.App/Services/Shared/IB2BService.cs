using RailwayWizzard.B2B;
using RailwayWizzard.Core;

namespace RailwayWizzard.App.Services.Shared
{
    public interface IB2BService
    {
        /// <summary>
        /// Проверяет существует ли такая станция по полному или неполному соответствию.
        /// </summary>
        /// <param name="stationName">Станция введенная пользователем.</param>
        /// <returns>Список станций.</returns>
        public Task<IReadOnlyCollection<StationInfo>> StationValidateAsync(string stationName);
            
        /// <summary>
        /// Возвращает актуальное расписание для указанного рейса.
        /// </summary>
        /// <param name="scheduleDto">Рейс.</param>
        /// <returns>Актуальное расписание.</returns>
        public Task<IReadOnlyCollection<string>> GetAvailableTimesAsync(ScheduleDto scheduleDto);
    }
}
