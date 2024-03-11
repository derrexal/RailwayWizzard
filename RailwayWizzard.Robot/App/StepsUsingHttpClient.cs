﻿using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using RzdHack.Robot.Core;


namespace RzdHack.Robot.App
{
    public class StepsUsingHttpClient : ISteps
    {
        private const string API_BOT_URL = "http://bot_service:5000/";
        private readonly ILogger _logger;

        public StepsUsingHttpClient(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Notification(NotificationTask input)
        {
            string railwayDataText = $"{input.DepartureStation} - {input.ArrivalStation} {input.TimeFrom} {input.DateFrom.ToString("dd.MM.yyy", CultureInfo.InvariantCulture)}";
            int count = 1;
            try
            {
                while (true)
                {
                    _logger.LogTrace($"Задача: {input.Id} Рейс: {railwayDataText} Попытка номер: {count}");
                    var freeSeats = await GetFreeSeats(input);

                    // Формируется текст уведомления о наличии мест
                    ResponseToUser messageToUser = new ResponseToUser
                    {
                        Message = $"{char.ConvertFromUtf32(0x2705)} {railwayDataText} " +
                                  $"\n{String.Join("\n", freeSeats.ToArray())}" +
                                  "\nОбнаружены свободные места\n",
                        UserId = input.UserId
                    };

                    //TODO:Вынести в метод? А лучше в отдельный класс взаимодействия с АПИ бота, так же как сделано и там
                    HttpClient httpClient = new HttpClient();
                    // определяем данные запроса
                    using HttpRequestMessage request =
                        new HttpRequestMessage(HttpMethod.Post, API_BOT_URL + "api/sendMessageForUser");
                    request.Content = JsonContent.Create(messageToUser);
                    // выполняем запрос
                    var response = await httpClient.SendAsync(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                        //Это погасит весь метод!
                        throw new Exception("Не удалось отправить сообщение пользователю");
                    count++;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Notification method. Task data:{railwayDataText}");
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task<List<string>> GetFreeSeats(NotificationTask task)
        {
            Robot robot = new(_logger);
            List<string> result;
            int count = 0;
            try
            {
                do
                {
                    //TODO: Переделать на более приближенный способ перезапуска задач
                    if (count >= 10) // 1.5 * 10 = 15 мин (1 раз в 16 минут запускается воркер, 1 минута - перекур))
                        throw new Exception($"Поток {Thread.CurrentThread.ManagedThreadId} закончил свое выполнение");
                    result = await robot.GetTicket(task);
                    count++;
                    Thread.Sleep(1000 * 30 * 3); //1.5 минуты
                }
                while (result.Count == 0);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return result;
        }
    }
}
