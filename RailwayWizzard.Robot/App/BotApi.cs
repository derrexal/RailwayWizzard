﻿using Microsoft.Extensions.Configuration;
using RailwayWizzard.Robot.Core;
using System.Net;
using System.Net.Http.Json;

namespace RailwayWizzard.Robot.App;

/// <summary>
/// Описывает методы взаимодействия с ботом
/// </summary>
public class BotApi : IBotApi
{
    private const string API_BOT_SEND_MESSAGE_URL = "http://bot_service:5000/api/sendMessageForUser";
    private const int DEFAULT_DELAY_TIME = 5000;

    private readonly IConfiguration _configuration;

    public BotApi(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Метод отправки сообщения пользователю
    /// </summary>
    /// <param name="message"></param>
    /// <param name="userId"></param>
    public async Task SendMessageForUserAsync(string message, long userId)
    {
        ResponseToUser messageToUser = new ResponseToUser{ Message = message, UserId = userId };
        using HttpClient httpClient = new HttpClient();
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_BOT_SEND_MESSAGE_URL);
        request.Content = JsonContent.Create(messageToUser);

        var response = await httpClient.SendAsync(request);
        
        // Retry send
        if (response.StatusCode != HttpStatusCode.OK)
        {
            await Task.Delay(DEFAULT_DELAY_TIME);
            response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Метод отправки сообщения пользователю:{userId} завершился с кодом:{response.StatusCode}.\nСообщение:{message}.\nОтвет сервера:{response}");
        }
    }
    
    /// <summary>
    /// Метод отправки сообщения админу
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendMessageForAdminAsync(string message)
    {
        var adminIdString = _configuration.GetSection("Telegram").GetSection("AdminId").Value;
        if (string.IsNullOrEmpty(adminIdString)) throw new Exception("Не задан телеграм ID администратора");
        long adminId = Convert.ToInt64(adminIdString);
        await SendMessageForUserAsync(message, adminId);
    }
}