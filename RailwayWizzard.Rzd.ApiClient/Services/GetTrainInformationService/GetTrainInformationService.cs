using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService.Models;

namespace RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService;

/// <inheritdoc/>
public class GetTrainInformationService: BaseGetDataService, IGetTrainInformationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrainInformationService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика HTTP клиентов.</param>
    public GetTrainInformationService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {}

    /// <inheritdoc/>
    public async Task<string> GetDataAsync(GetTrainInformationRequest request)
    {
        var (departureStationCode, arrivalStationCode, departureDateTime, token, getTrainsFromSchedule) = request;

        var url = $"https://ticket.rzd.ru/apib2b/p/Railway/V1/Search/TrainPricing?service_provider=B2B_RZD&bs={token}";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        
        requestMessage.Headers.Add("Accept", "application/json, text/plain, */*");
        requestMessage.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9");
        requestMessage.Headers.Add("Connection", "keep-alive");
        requestMessage.Headers.Add("Origin", "https://ticket.rzd.ru");
        requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        requestMessage.Headers.Add("Cookie", " LANG_SITE=ru; " + $"oxxfgh={token}");

        requestMessage.Content = new StringContent(
            "{" +
            "\"Origin\":\"" + departureStationCode + "\"," +
            "\"Destination\":\"" + arrivalStationCode + "\"," +
            "\"DepartureDate\":\"" + departureDateTime.ToString("yyyy-MM-ddT00:00:00") + "\"," +
            "\"TimeFrom\":0," +
            "\"TimeTo\":24," +
            "\"CarGrouping\":\"DontGroup\"," +
            "\"GetByLocalTime\":true," +
            "\"SpecialPlacesDemand\":\"StandardPlacesAndForDisabledPersons\"," +
            "\"CarIssuingType\":\"All\"," +
          //"\"GetTrainsFromSchedule\":true}"
            "\"GetTrainsFromSchedule\":" +
            getTrainsFromSchedule.ToString().ToLower() +
            "}"
            , null
            , "application/json");

        return await BaseHttpSenderAsync(requestMessage);
    }
}