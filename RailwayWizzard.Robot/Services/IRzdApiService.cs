namespace RailwayWizzard.Rzd.DataEngine.Services;

/// <summary>
/// Сервис для взаимодействия с АПИ РЖД.
/// </summary>
public interface IRzdApiService
{
    public Task<IReadOnlyCollection<StationFromJson>> GetStationsAsync(string inputStation);
}