namespace RailwayWizzard.Rzd.ApiClient.Services.GetStationDetailsService;

/// <summary>
/// Сервис получения детализации о станции.
/// </summary>
public interface IGetStationDetailsService
{
    /// <summary>
    /// Метод получения данных.
    /// </summary>
    /// <param name="stationName">Наименование станции.</param>
    /// <returns>Строка с ответом.</returns>
    public Task<string> GetDataAsync(string stationName);
}