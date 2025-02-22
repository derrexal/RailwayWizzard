namespace RailwayWizzard.Rzd.ApiClient.Services.GetStationDetailsService;

/// <inheritdoc/>
public class GetStationDetailsService: BaseGetDataService, IGetStationDetailsService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetStationDetailsService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика HTTP клиентов.</param>
    public GetStationDetailsService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {}

    /// <inheritdoc/>
    public async Task<string> GetDataAsync(string stationName)
    {
        var requestUri = $"https://ticket.rzd.ru/api/v1/suggests?Query={stationName}&TransportType=bus,avia,rail,aeroexpress,suburban,boat&GroupResults=true&RailwaySortPriority=true&SynonymOn=1";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        request.Headers.Add("Accept", "application/json, text/plain, */*");
        request.Headers.Add("Accept-Language", "ru,en;q=0.9");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 YaBrowser/24.7.0.0 Safari/537.36");

        return await BaseHttpSenderAsync(request);
    }
}