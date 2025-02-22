using System.Text.RegularExpressions;

namespace RailwayWizzard.Rzd.ApiClient.Services.GetFirewallTokenService;

/// <inheritdoc/>
public class GetFirewallTokenService: BaseGetDataService, IGetFirewallTokenService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFirewallTokenService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика HTTP клиентов.</param>
    public GetFirewallTokenService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {}
    
    /// <inheritdoc/>
    public async Task<string> GetDataAsync()
    {
        const string url = "https://w-22900.fp.kaspersky-labs.com/oxwdsq?cid=22900";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await BaseHttpSenderAsync(request);
        
        return ExtractTokenFromResponse(response);
    }
    
    /// <summary>
    /// Извлекает токен из ответа сервера.
    /// </summary>
    /// <param name="response">Ответ сервера.</param>
    /// <returns>Токен.</returns>
    /// <exception cref="HttpRequestException">Ошибка невалидного ответа.</exception>
    private static string ExtractTokenFromResponse(string response)
    {
        try
        {
            //вытаскиваем из ответа строку со Ksid
            if (!response.Contains("id")) 
                throw new HttpRequestException($"Сервис Касперского вернул невалидный ответ:\n{response}");
            
            var regex = new Regex("\"id\":\"(.*?)\"");
            var res = regex.Match(response).ToString();

            //Из всей строки получаем только значение
            var keyValuePairs = res.Split(':').ToList();
            var result = keyValuePairs.LastOrDefault();
            if (result is null)
                throw new HttpRequestException($"Не удалось распарсить ответ от Касперского:\n{response}");

            result = result.Remove(result.Length - 1);
            result = result.Remove(0, 1);
            return result;
        }

        catch (HttpRequestException e)
        {
            throw new HttpRequestException($"Не доступен сервис Касперского для получения токена\n{e}");
        }
    }
}