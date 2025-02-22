using RailwayWizzard.Common;

namespace RailwayWizzard.Rzd.ApiClient.Services;

/// <summary>
/// Базовая имплементация сервиса получения данных.
/// </summary>
public class BaseGetDataService
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseGetDataService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика HTTP клиентов.</param>
    protected BaseGetDataService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected async Task<string> BaseHttpSenderAsync(HttpRequestMessage request)
    {
        using var client = _httpClientFactory.CreateClient();

        using var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var textResponse = await response.Content.ReadAsStringAsync();

        textResponse.IsNotNullOrEmptyOrWhiteSpace();

        return textResponse;
    }
}