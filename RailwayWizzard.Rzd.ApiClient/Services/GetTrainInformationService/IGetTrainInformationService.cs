using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService.Models;

namespace RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService;

/// <summary>
/// Сервис получения информации о запрашиваемом рейсе.
/// </summary>
public interface IGetTrainInformationService
{
    /// <summary>
    /// Метод получения данных.
    /// </summary>
    /// <param name="request">Запрос.</param>
    /// <returns>Строка с ответом.</returns>
    public Task<string> GetDataAsync(GetTrainInformationRequest request);

}