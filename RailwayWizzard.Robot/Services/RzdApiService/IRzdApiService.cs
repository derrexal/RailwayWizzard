using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService.Models;
using RailwayWizzard.Rzd.DataEngine.Core;

namespace RailwayWizzard.Rzd.DataEngine.Services;

/// <summary>
/// Сервис для взаимодействия с АПИ РЖД.
/// </summary>
public interface IRzdApiService
{
    public Task<IReadOnlyCollection<StationFromJson>> GetStationsAsync(string inputStation);

    public Task<Root> GetTrainInfoAsync(GetTrainInformationRequest request);
}