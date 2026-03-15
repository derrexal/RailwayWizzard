using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RailwayWizzard.Infrastructure.Repositories.StationsInfo;
using RailwayWizzard.Rzd.ApiClient.Services.GetStationsByNameService;

namespace RailwayWizzard.Rzd.DataEngine.Services;

/// <inheritdoc/>
public class RzdApiService
{
    private readonly IGetStationsByNameService _stationsByNameService;
    
    // private readonly ILogger<RzdApiService> _logger;
    
    public RzdApiService(IGetStationsByNameService stationsByNameService)
    {
        _stationsByNameService = stationsByNameService;
    }
    
    public async Task<IReadOnlyCollection<StationFromJson>> GetStationsAsync(string inputStation)
    {
        var textResponse = await _stationsByNameService.GetDataAsync(inputStation);
        if (string.IsNullOrEmpty(textResponse)) 
            return new List<StationFromJson>();
            
        var stations = DeserializeStationsText(textResponse);
            
        return stations;

    }
    
    private static List<StationFromJson> DeserializeStationsText(string textResponse)
    {
        var stations = JsonConvert.DeserializeObject<List<StationFromJson>>(textResponse);
            
        if (stations!.Count == 0)
            throw new NullReferenceException(
                $"Сервис РЖД при запросе списка свободных мест вернул ответ в котором нет поездок. Ответ:{textResponse}");

        return stations;
    }


}