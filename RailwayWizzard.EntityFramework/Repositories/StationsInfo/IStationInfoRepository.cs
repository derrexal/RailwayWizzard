using RailwayWizzard.Core.StationInfo;

namespace RailwayWizzard.Infrastructure.Repositories.StationsInfo
{
    /// <summary>
    /// Репозиторий сущности <see cref="StationInfoExtended"/>.
    /// </summary>
    public interface IStationInfoRepository
    {
        /// <summary>
        /// Получить станцию по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор станции.</param>
        /// <returns>Станция.</returns>
        public Task<StationInfoExtended> GetByIdAsync(int id);
        
        /// <summary>
        /// Получить станцию по наименованию (полное совпадение).
        /// </summary>
        /// <param name="name">Наименование станции.</param>
        /// <returns>Найденная станция или ошибка.</returns>
        public Task<StationInfoExtended> GetByNameAsync(string name);
        
        /// <summary>
        /// Найти станцию по наименованию (полному совпадению).
        /// </summary>
        /// <param name="name">Наименование станции.</param>
        /// <returns>Найденная станция или null.</returns>
        public Task<StationInfoExtended?> FindByNameExactAsync(string name);

        /// <summary>
        /// Найти множество станций по наименованию (по вхождению).
        /// </summary>
        /// <param name="name">Наименование станции.</param>
        /// <returns>Коллекция найденных станций.</returns>
        public Task<IReadOnlyCollection<StationInfoExtended>> FindByNameContainsAsync(string name);

        /// <summary>
        /// Добавить коллекцию станций.
        /// </summary>
        /// <param name="stationInfos">Коллекция станций.</param>
        /// <returns>Результат.</returns>
        public Task AddRangeStationInfosAsync(IReadOnlyCollection<StationInfoExtended> stationInfos);
    }
}
