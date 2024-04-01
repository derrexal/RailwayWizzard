using RailwayWizzard.Robot.Core;
using System.Net;
using System.Net.Http.Json;

namespace RailwayWizzard.Robot.App;

public class BotApi: IBotApi
{
    // private const string API_BOT_URL = "http://bot_service:5000/";
    private const string API_BOT_SEND_MESSAGE_URL = "http://bot_service:5000/api/sendMessageForUser";

    /// <summary>
    /// Метод отправки сообщения пользователю
    /// </summary>
    /// <param name="message"></param>
    /// <param name="userId"></param>
    public async Task SendMessageForUser(string message, long userId)
    {
        ResponseToUser messageToUser = new ResponseToUser{ Message = message, UserId = userId };

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_BOT_SEND_MESSAGE_URL);
        request.Content = JsonContent.Create(messageToUser);

        try
        {
            using HttpClient httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Метод отправки сообщения пользователю:{userId} завершился с кодом:{response.StatusCode}");
        }
        catch{ throw; }
    }
}