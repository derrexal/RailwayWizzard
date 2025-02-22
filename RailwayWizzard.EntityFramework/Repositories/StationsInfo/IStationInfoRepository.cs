using RailwayWizzard.Core.StationInfo;

namespace RailwayWizzard.Infrastructure.Repositories.StationsInfo
{
    /// <summary>
    /// Репозиторий сущности <see cref="StationInfo"/>.
    /// </summary>
    public interface IStationInfoRepository
    {
        /// <summary>
        /// Получить станцию по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор станции.</param>
        /// <returns>Станция.</returns>
        public Task<StationInfo> GetByIdAsync(int id);
        
        /// <summary>
        /// Получить станцию по наименованию (полное совпадение).
        /// </summary>
        /// <param name="name">Наименование станции.</param>
        /// <returns>Найденная станция или ошибка.</returns>
        public Task<StationInfo> GetByNameAsync(string name);
        
        /// <summary>
        /// Найти станцию по наименованию (полному совпадению).
        /// </summary>
        /// <param name="name">Наименование станции.</param>
        /// <returns>Найденная станция или null.</returns>
        public Task<StationInfo?> FindByNameAsync(string name);

        /// <summary>
        /// Найти множество станций по наименованию (по вхождению).
        /// </summary>
        /// <param name="name">Наименование станции.</param>
        /// <returns>Коллекция найденных станций.</returns>
        public Task<IReadOnlyCollection<StationInfo>> ContainsByStationNameAsync(string name);

        /// <summary>
        /// Проверить существование станции по ее коду.
        /// </summary>
        /// <param name="expressCode">Код станции.</param>
        /// <returns>Результат.</returns>
        public Task<bool> AnyByExpressCodeAsync(long expressCode);

        /// <summary>
        /// Добавить множество станций.
        /// </summary>
        /// <param name="stations">Множество станций.</param>
        /// <returns>Результат.</returns>
        public Task AddRangeStationInfoAsync(IReadOnlyCollection<StationInfo> stations);
    }
}
