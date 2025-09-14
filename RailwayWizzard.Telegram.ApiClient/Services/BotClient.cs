using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using RailwayWizzard.Common;
using RailwayWizzard.Telegram.ApiClient.Exceptions;
using RailwayWizzard.Telegram.ApiClient.Models;

namespace RailwayWizzard.Telegram.ApiClient.Services;

/// <inheritdoc/>
public class BotClient : IBotClient
{
    private const string API_BOT_SEND_MESSAGE_URL = "http://bot_service:5000/api/sendMessageForUser";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BotClient> _logger;

    public BotClient(IHttpClientFactory httpClientFactory, ILogger<BotClient> logger)
    {
        _httpClientFactory = Ensure.NotNull(httpClientFactory);
        _logger = Ensure.NotNull(logger);
    }

    /// <inheritdoc/>
    public async Task SendMessageForUserAsync(MessageDto messageDto)
    {
        var (message, telegramUserId) = messageDto; 
        
        using var request = new HttpRequestMessage(HttpMethod.Post, API_BOT_SEND_MESSAGE_URL);
        request.Content = JsonContent.Create(messageDto);

        using var httpClient = _httpClientFactory.CreateClient(); ;
        
        using var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Сообщение пользователю успешно отправлено. " +
                             $"Дополнительная информация: Message: {message} User: {telegramUserId}");
        }
        
        else if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var errorDetail = await response.Content.ReadAsStringAsync();
            
            _logger.LogWarning("Сообщение пользователю не отправлено. Пользователь заблокировал бота. " +
                               $"Дополнительная информация: Message: {message} User: {telegramUserId} Детали ошибки: {errorDetail}");

            throw new ConflictHttpException(errorDetail);
        }
        
        else
        {
            var errorDetail = await response.Content.ReadAsStringAsync();
            
            _logger.LogWarning("Сообщение пользователю не отправлено. Произошла неизвестная ошибка. " +
                               $"Дополнительная информация: Message: {message} User: {telegramUserId} Детали ошибки: {errorDetail}");
            
            throw new HttpRequestException(errorDetail);
        }
    }
}