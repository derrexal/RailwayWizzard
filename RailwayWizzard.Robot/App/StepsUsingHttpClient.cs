using Microsoft.Extensions.Logging;
using RailwayWizzard.Core;
using RailwayWizzard.Shared;
using System.Text;


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
            string notificationTaskText = inputNotificationTask.ToCustomString();
            string logMessage = $"Задача: {inputNotificationTask.Id} Рейс: {notificationTaskText}";

            try
            {
                // Задача помечается статусом "В работе"
                await _checker.SetIsWorkedNotificationTask(inputNotificationTask);

                _logger.LogTrace($"Run {logMessage} in Thread:{Thread.CurrentThread.ManagedThreadId}");
                    
                //Если во время выполнения задача стала неактуальна
                if (!_checker.CheckActualNotificationTask(inputNotificationTask))
                {
                    await _checker.SetIsNotActualAndIsNotWorked(inputNotificationTask);
                    _logger.LogTrace($"Во время выполнения программы задача {inputNotificationTask.Id} " +
                                        $"стала неактуальна. Подробности задачи:{notificationTaskText}");
                    return;
                }

                // Поиск свободных мест
                var freeSeats = await _robot.GetFreeSeatsOnTheTrain(inputNotificationTask);
                var resultFreeSeats = String.Join(";", freeSeats);

                //Если текущий результат равен предыдущему - завершаем задачу
                if (await _checker.ResultIsLast(inputNotificationTask, resultFreeSeats!)) return;

                // Формирование текста уведомления о наличии мест
                StringBuilder message = new();

                // Если свободные места были, а сейчас их уже нет
                if (resultFreeSeats == "")
                    message = message.Append($"{char.ConvertFromUtf32(0x26D4)} {notificationTaskText}" + 
                               "\n Свободных мест больше нет");
                // Если свободных мест не было, а сейчас они появились
                else
                    message = message.Append($"{char.ConvertFromUtf32(0x2705)} {notificationTaskText}" +
                              $"\n{String.Join("\n", freeSeats.ToArray())}" +
                              "\nОбнаружены свободные места\n");
                    
                // Отправка сообщения пользователю
                await _botApi.SendMessageForUserAsync(message.ToString(), inputNotificationTask.UserId);
                
                //TODO: Не нужно хранить всю строку, можно записывать н-р хэш
                // Записываем информацию о результате поиска свободных мест
                await _checker.SetLastResultNotificationTask(inputNotificationTask, resultFreeSeats!);

                //Задача закончила свое выполнение
                await _checker.SetIsNotWorked(inputNotificationTask);

                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                await _checker.SetIsNotWorked(inputNotificationTask);
                _logger.LogError($"Неизвестная ошибка метода обработки задач. {logMessage}\n {e}");
                throw;
            }
        }
    }
}