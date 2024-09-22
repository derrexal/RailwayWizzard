using RailwayWizzard.Core;

namespace RailwayWizzard.EntityFrameworkCore.Repositories.StationInfos
{
    /// <summary>
    /// Репозиторий сущности <see cref="StationInfo"/>.
    /// </summary>
    public interface IStationInfoRepository
    {
        /// <summary>
        /// Найти единственную станцию по полному совпадению.
        /// </summary>
        /// <param name="stationName">Наименование станции.</param>
        /// <returns>Найденная станция или null.</returns>
        public Task<StationInfo?> FindByStationNameAsync(string stationName);

        /// <summary>
        /// Найти множество станций содержащих в наименовании указанную строку.
        /// </summary>
        /// <param name="stationName">Имя станции по которому производится поиск.</param>
        /// <returns>Коллекция найденных станций.</returns>
        public Task<IReadOnlyCollection<StationInfo>> ContainsByStationNameAsync(string stationName);

        /// <summary>
        /// Проверить существование станции по коду.
        /// </summary>
        /// <param name="expressCode">Код станции.</param>
        /// <returns>Результат.</returns>
        public Task<bool> AnyByExpressCodeAsync(long expressCode);

        /// <summary>
        /// Добавить множество станций в БД.
        /// </summary>
        /// <param name="stations">Множество станций.</param>
        /// <returns>Результат.</returns>
        public Task AddRangeStationInfoAsync(IReadOnlyCollection<StationInfo> stations);
    }
}
