namespace RailwayWizzard.Rzd.ApiClient.Services.GetStationsByNameService;

/// <summary>
/// Сервис получения информации о станции по наименованию.
/// </summary>
public interface IGetStationsByNameService
{
    /// <summary>
    /// Метод получения данных.
    /// </summary>
    /// <param name="request">Запрос (минимум 2 символа).</param>
    /// <returns>Строка с ответом.</returns>
    public Task<string> GetDataAsync(string request);
    
    /// <summary>
    /// Новый метод получения расширенных данных.
    /// </summary>
    /// <param name="request">Запрос (минимум 2 символа).</param>
    /// <returns>Строка с ответом.</returns>
    public Task<string> GetDataExtendedAsync(string request);
}