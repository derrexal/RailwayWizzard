﻿using Microsoft.Extensions.Logging;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Shared;


namespace RailwayWizzard.Robot.App
{
    public class StepsUsingHttpClient : ISteps
    {
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
            // Счетчик успешных попыток
            int count = 1;
            string railwayDataText = inputNotificationTask.ToCustomString();
            string messageNotification = $"Задача: {inputNotificationTask.Id} Попытка: {count} Рейс: {railwayDataText}";

            try
            {
                await using (var context = await _contextFactory.CreateDbContextAsync())
                {
                    var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                    currentNotificationTask!.IsWorked = true;
                    await context.SaveChangesAsync();
                }

                while (true)
                {
                    _logger.LogTrace(messageNotification);
                    
                    //Если во время выполнения задача стала неактуальна
                    if (!_checker.CheckActualNotificationTask(inputNotificationTask))
                    {
                        await using (var context = await _contextFactory.CreateDbContextAsync())
                        {
                            var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                            currentNotificationTask!.IsActual = false;
                            currentNotificationTask!.IsWorked = false;
                            await context.SaveChangesAsync();
                        }
                        _logger.LogTrace($"Во время выполнения программы задача {inputNotificationTask.Id} " +
                                         $"стала неактуальна. Подробности задачи:{railwayDataText}");
                        break;
                    }                    

                    //TODO: Если непосредствено после вызова этого метода пользователь выставит статус "Не актуально"
                    //TODO: Метод будет зря крутится. 
                    //TODO: Вынести его внутрянку сюда(ну или хотя бы цикл убрать и проверять здесь? )
                    //TODO: Придется намного чаще будем ходить в БД чтобы посмотреть статус - но разве ни этого мы добиваемся)
                    var freeSeats = await GetFreeSeats(inputNotificationTask);

                    //Если задача остановлена пользователем.
                    //Расположил эту конструкцию перед отправкой сообщения, чтобы наверняка
                    await using (var context = await _contextFactory.CreateDbContextAsync())
                    {
                        var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                        if (currentNotificationTask!.IsStopped)
                        {
                            _logger.LogTrace($"Во время выполнения программы задача {inputNotificationTask.Id} " +
                                             $"была остановлена пользователем. Подробности задачи:{railwayDataText}");
                            break;
                        }
                    }

                    var botApi = new BotApi();
                    // Формирование текста уведомления о наличии мест
                    string message = $"{char.ConvertFromUtf32(0x2705)} {railwayDataText}" +
                                     $"\n{String.Join("\n", freeSeats.ToArray())}" +
                                     "\nОбнаружены свободные места\n";
                    // Отправка сообщения пользователю
                    await botApi.SendMessageForUser(message, inputNotificationTask.UserId);
                                            
                    count++;
                }
            }
            catch (Exception e)
            {
                await using (var context = await _contextFactory.CreateDbContextAsync())
                {
                    var currentNotificationTask = await context.NotificationTask.FirstOrDefaultAsync(t => t.Id == inputNotificationTask.Id);
                    currentNotificationTask!.IsWorked = false;
                    await context.SaveChangesAsync();
                }

                _logger.LogError($"Неизвестная ошибка метода обработки задач. {messageNotification}/n {e}");
                throw;
            }
        }
        
        /// <summary>
        /// Метод получения свободных мест по заданным параметрам поездки
        /// Возвращает результат только если места есть
        /// В противном случае - крутится в цикле до момента их появления.
        /// </summary>
        /// <param name="inputNotificationTask"></param>
        /// <returns></returns>
        private async Task<List<string>> GetFreeSeats(NotificationTask inputNotificationTask)
        {
            Robot robot = new(_logger);
            List<string> result;
            try
            {
                do
                {
                    result = await robot.GetTicket(inputNotificationTask);
                    Thread.Sleep(1000 * 30); //пол минуты
                }
                while (result.Count == 0);
            }
            catch { throw; }

            return result;
        }
    }
}
