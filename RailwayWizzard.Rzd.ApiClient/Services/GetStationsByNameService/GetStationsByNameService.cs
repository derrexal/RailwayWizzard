namespace RailwayWizzard.Rzd.ApiClient.Services.GetStationsByNameService;

/// <inheritdoc cref="IGetStationsByNameService"/>/>
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
        var inputStationUri = Uri.EscapeDataString(inputStation);

        var url =
            $"https://ticket.rzd.ru/api/v1/suggests?" +
            $"GroupResults=true" +
            $"&RailwaySortPriority=true" +
            $"&MergeSuburban=true" +
            $"&Query={inputStationUri}" +
            $"&Language=ru" +
            $"&TransportType=rail,suburban,boat,bus,aeroexpress"; // avia сознательно не запрашиваем 

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Host", "pass.rzd.ru");

        return await BaseHttpSenderAsync(request);
    }
}