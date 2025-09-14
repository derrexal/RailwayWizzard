using RailwayWizzard.Application.Dto.B2B;
using RailwayWizzard.Core.StationInfo;

namespace RailwayWizzard.Application.Services.B2B
{
    public interface IB2BService
    {
        /// <summary>
        /// Ищет станцию по наименованию по полному или неполному соответствию.
        /// </summary>
        /// <param name="stationName">Наименование станции введенное пользователем.</param>
        /// <returns>Список найденных станций.</returns>
        public Task<IReadOnlyCollection<StationInfoExtended>> StationValidateAsync(string stationName);

        /// <summary>
        /// Возвращает актуальное расписание для указанного рейса.
        /// </summary>
        /// <param name="scheduleDto">Рейс.</param>
        /// <returns>Актуальное расписание.</returns>
        public Task<IReadOnlyCollection<string>> GetAvailableTimesAsync(RouteDto scheduleDto);
    }
}
