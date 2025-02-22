namespace RailwayWizzard.Rzd.ApiClient.Services.GetStationsByNameService;

/// <inheritdoc/>
public class GetStationsByNameService: BaseGetDataService, IGetStationsByNameService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetStationsByNameService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика HTTP клиентов.</param>
    public GetStationsByNameService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {}
    
    /// <inheritdoc/>
    public async Task<string> GetDataAsync(string inputStation)
    {
        var uriInputStation = Uri.EscapeDataString(inputStation);

        var url = $"https://pass.rzd.ru/suggester/?stationNamePart={uriInputStation}&lang=ru";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Host", "pass.rzd.ru");

        return await BaseHttpSenderAsync(request);
    }

}