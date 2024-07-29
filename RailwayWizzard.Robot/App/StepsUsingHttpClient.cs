using Microsoft.Extensions.Logging;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Shared;


namespace RailwayWizzard.Robot.App
{
    public class StepsUsingHttpClient : ISteps
    {
        private readonly IRobot _robot;
        private readonly IBotApi _botApi;
        private readonly IChecker _checker;
        private readonly ILogger<StepsUsingHttpClient> _logger;
        
        public StepsUsingHttpClient(
            IRobot robot,
            IBotApi botApi,
            IChecker checker,
            ILogger<StepsUsingHttpClient> logger)
        {
            _robot = robot;
            _botApi = botApi;
            _checker = checker;
            _logger = logger;
        }

        // TODO: сделать что-то с тем, что пользователи заблокировал бота...
        // Если выяснится что пользователь заблокировал бота - выставить ему в таблице юзеров статус - IsBlocked.
        // Когда пользователь вновь написал боту (Users/CreateOrUpdate) - выставляем ему статус IsBlocked=false
        public async Task Notification(NotificationTask inputNotificationTask)
        {    
            
            int count = 1;  // Счетчик успешных попыток
            string notificationTaskText = inputNotificationTask.ToCustomString();
            string logMessage = $"Задача: {inputNotificationTask.Id} Попытка: {count} Рейс: {notificationTaskText}";
            
            try
            {
                await _checker.SetIsWorkedNotificationTask(inputNotificationTask);

                while (true)
                {
                    _logger.LogTrace(logMessage);
                    
                    //Если во время выполнения задача стала неактуальна
                    if (!_checker.CheckActualNotificationTask(inputNotificationTask))
                    {
                        await _checker.SetIsNotActualAndIsNotWorked(inputNotificationTask);
                        _logger.LogTrace($"Во время выполнения программы задача {inputNotificationTask.Id} " +
                                         $"стала неактуальна. Подробности задачи:{notificationTaskText}");
                        break;
                    }

                    var freeSeats = await GetFreeSeats(inputNotificationTask);
                    if (freeSeats.Count == 0) break;

                    // Формирование текста уведомления о наличии мест
                    string message = $"{char.ConvertFromUtf32(0x2705)} {notificationTaskText}" +
                                     $"\n{String.Join("\n", freeSeats.ToArray())}" +
                                     "\nОбнаружены свободные места\n";
                    
                    // Отправка сообщения пользователю
                    await _botApi.SendMessageForUserAsync(message, inputNotificationTask.UserId);
                                            
                    count++;
                }
            }
            catch (Exception e)
            {
                await _checker.SetIsNotWorked(inputNotificationTask);
                _logger.LogError($"Неизвестная ошибка метода обработки задач. {logMessage}\n {e}");
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
            List<string> result = new();
            string railwayDataText = inputNotificationTask.ToCustomString();
            do
            {
                Thread.Sleep(1000 * 30); //пол минуты
                
                //Если задача остановлена пользователем - останавливаем поиск
                var IsStoppedNotificationTask = await _checker.GetIsStoppedNotificationTask(inputNotificationTask);
                if (IsStoppedNotificationTask)
                {
                    await _checker.SetIsNotWorked(inputNotificationTask);
                    _logger.LogTrace($"Во время выполнения программы задача {inputNotificationTask.Id} " +
                                        $"была остановлена пользователем. Подробности задачи:{railwayDataText}");
                    return result;
                }

                result = await _robot.GetFreeSeatsOnTheTrain(inputNotificationTask);
            }
            while (result.Count==0);

            return result;
        }
    }
}
