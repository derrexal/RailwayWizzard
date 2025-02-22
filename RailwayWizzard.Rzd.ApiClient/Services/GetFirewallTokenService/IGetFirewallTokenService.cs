namespace RailwayWizzard.Rzd.ApiClient.Services.GetFirewallTokenService;

/// <summary>
/// Сервис получения токена от фаервола.
/// Токен необходим для дальнейших запросов. 
/// </summary>
public interface IGetFirewallTokenService
{
    public Task<string> GetDataAsync();
}