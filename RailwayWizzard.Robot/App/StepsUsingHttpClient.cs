﻿using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using RailwayWizzard.Robot.Core;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Shared;


namespace RailwayWizzard.Robot.App
{
    public class StepsUsingHttpClient : ISteps
    {
        private const string API_BOT_URL = "http://bot_service:5000/";
        private readonly IChecker _checker;
        private readonly ILogger _logger;
        private readonly IDbContextFactory<RailwayWizzardAppContext> _contextFactory;

        public StepsUsingHttpClient(
            IChecker checker,
            ILogger logger, 
            IDbContextFactory<RailwayWizzardAppContext> contextFactory)
        {
            _checker = checker;
            _logger = logger;
            _contextFactory = contextFactory;
        }

        public async Task Notification(NotificationTask inputNotificationTask)
        {
            int count = 1;
            string railwayDataText = $"{inputNotificationTask.DepartureStation} - {inputNotificationTask.ArrivalStation} {inputNotificationTask.TimeFrom} {inputNotificationTask.DateFrom.ToString("dd.MM.yyy", CultureInfo.InvariantCulture)}";
            string messageNotification = $"Задача: {inputNotificationTask.Id} Рейс: {railwayDataText} Попытка номер: {count}";

            try
            {
                using (var _context = _contextFactory.CreateDbContext())
                {
                    var currentNotificationTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                    currentNotificationTask!.IsWorked = true;
                    await _context.SaveChangesAsync();
                }

                while (true)
                {

                    //Если во время выполнения задача стала неактуальна
                    if (!_checker.CheckActualNotificationTask(inputNotificationTask))
                    {
                        using (var _context = _contextFactory.CreateDbContext())
                        {
                            var currentNotificationTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                            currentNotificationTask!.IsActual = false;
                            currentNotificationTask!.IsWorked = false;
                            await _context.SaveChangesAsync();
                        }
                        break;
                    }                    

                    _logger.LogTrace(messageNotification);
                    var freeSeats = await GetFreeSeats(inputNotificationTask);

                    //Если задача остановлена пользователем. Расположил после наиболее "долгого" места в методе
                    using (var _context = _contextFactory.CreateDbContext())
                    {
                        var currentNotificationTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                        if (currentNotificationTask!.IsStopped)
                            break;
                    }

                    // Формируется текст уведомления о наличии мест
                    ResponseToUser messageToUser = new ResponseToUser
                    {
                        Message = $"{char.ConvertFromUtf32(0x2705)} {railwayDataText}" +
                                  $"\n{String.Join("\n", freeSeats.ToArray())}" +
                                  "\nОбнаружены свободные места\n",
                        UserId = inputNotificationTask.UserId
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
                using (var _context = _contextFactory.CreateDbContext())
                {
                    var currentNotificationTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                    currentNotificationTask!.IsWorked = false;
                    await _context.SaveChangesAsync();
                }

                _logger.LogError($"Ошибка в методе Notification. {messageNotification}");
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task<List<string>> GetFreeSeats(NotificationTask inputNotificationTask)
        {
            Robot robot = new(_logger);
            List<string> result;
            try
            {
                do
                {
                    result = await robot.GetTicket(inputNotificationTask);
                    Thread.Sleep(1000 * 30 * 3); //1.5 минуты
                }
                while (result.Count == 0);
            }
            catch { throw; }

            return result;
        }
    }
}
