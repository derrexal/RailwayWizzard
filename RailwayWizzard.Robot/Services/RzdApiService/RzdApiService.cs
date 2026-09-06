// using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RailwayWizzard.Rzd.ApiClient.Services.GetStationsByNameService;
using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService;
using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService.Models;
using RailwayWizzard.Rzd.DataEngine.Core;
using RailwayWizzard.Rzd.DataEngine.Exceptions;

namespace RailwayWizzard.Rzd.DataEngine.Services;

/// <inheritdoc/>
public class RzdApiService
{
    private readonly IGetStationsByNameService _stationsByNameService;
    private readonly IGetTrainInformationService _trainInformationService;

    // private readonly ILogger<RzdApiService> _logger;

    public RzdApiService(
        IGetStationsByNameService stationsByNameService, 
        IGetTrainInformationService trainInformationService)
    {
        _stationsByNameService = stationsByNameService;
        _trainInformationService = trainInformationService;
    }

    public async Task<IReadOnlyCollection<StationFromJson>> GetStationsAsync(string inputStation)
    {
        var textResponse = await _stationsByNameService.GetDataAsync(inputStation);

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

    public async Task<Root> GetTrainInfoAsync(GetTrainInformationRequest request)
    {
        var trainInfoResponse = await _trainInformationService.GetDataAsync(request);

        CheckTrainInfoResponseAndThrow(trainInfoResponse);

        var trainInfo = JsonConvert.DeserializeObject<Root>(trainInfoResponse);
        if (trainInfo?.Id == null)
            throw new UnknownResponseException(
                $"Сервис РЖД при запросе списка свободных мест вернул не стандартный ответ. Ответ:{trainInfoResponse}",
                null);
        if (trainInfo.Trains.Count == 0)
        {
            throw new TrainNotFoundException(TrainNotFoundException.noTrainsMessage, null);
        }

        return trainInfo;
    }

    //TODO: Переделать на определение состояния по Code
    /// Билеты перестают продавать за определенное время. Обрабатываем эти и другие ситуации.
    private static void CheckTrainInfoResponseAndThrow(string trainInfoResponse)
    {
        if (trainInfoResponse.Contains(TrainNotFoundException.noTrainsMessage))
        {
            throw new TrainNotFoundException(TrainNotFoundException.noTrainsMessage, null);
        }
        if (trainInfoResponse.Contains(NoPlaceException.noPlaceMessage))
        {
            throw new NoPlaceException(NoPlaceException.noPlaceMessage, null);
        }
        if (trainInfoResponse.Contains(DontRunException.dontRunMessage))
        {
            throw new DontRunException(DontRunException.dontRunMessage, null);
        }
    }
}