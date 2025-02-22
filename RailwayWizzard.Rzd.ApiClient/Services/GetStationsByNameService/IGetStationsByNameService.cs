namespace RailwayWizzard.Rzd.ApiClient.Services.GetStationsByNameService;

/// <summary>
/// Сервис получения информации о станциях по имени.
/// </summary>
public interface IGetStationsByNameService
{
    /// <summary>
    /// Метод получения данных.
    /// </summary>
    /// <param name="request">Запрос.</param>
    /// <returns>Строка с ответом.</returns>
    public Task<string> GetDataAsync(string request);
}