using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RailwayWizzard.Robot.Core;
using System.Net.Http.Json;

namespace RailwayWizzard.Robot.App;

/// <inheritdoc/>
public class BotClient : IBotClient
{
    private const string API_BOT_SEND_MESSAGE_URL = "http://bot_service:5000/api/sendMessageForUser";

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BotClient> _logger;

    public BotClient(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<BotClient> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendMessageForUserAsync(string message, long userId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, API_BOT_SEND_MESSAGE_URL);
        request.Content = JsonContent.Create(new ResponseToUser { Message = message, UserId = userId });

        using var httpClient = _httpClientFactory.CreateClient(); ;
        
        using var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            _logger.LogError($"Метод отправки сообщения пользователю завершился с кодом {response.StatusCode}. Message: {message} User: {userId}");
    }

    /// <inheritdoc/>
    public async Task SendMessageForAdminAsync(string message)
    {
        var adminIdString = _configuration.GetSection("Telegram").GetSection("AdminId").Value;
        if (string.IsNullOrEmpty(adminIdString)) throw new Exception("Не задан телеграм ID администратора");
        long adminId = Convert.ToInt64(adminIdString);
        await SendMessageForUserAsync(message, adminId);
    }
}