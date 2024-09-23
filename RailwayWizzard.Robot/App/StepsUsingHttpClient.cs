using Microsoft.Extensions.Logging;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.UnitOfWork;

namespace RailwayWizzard.Robot.App
{
    public class StepsUsingHttpClient : ISteps
    {
        private readonly IRobot _robot;
        private readonly IBotApi _botApi;
        private readonly IRailwayWizzardUnitOfWork _uow;
        private readonly ILogger<StepsUsingHttpClient> _logger;
        
        public StepsUsingHttpClient(
            IRobot robot,
            IBotApi botApi,
            IRailwayWizzardUnitOfWork uow,
            ILogger<StepsUsingHttpClient> logger)
        {
            _robot = robot;
            _botApi = botApi;
            _uow = uow;
            _logger = logger;
        }

        // TODO: сделать что-то с тем, что пользователи заблокировал бота...
        // Если выяснится что пользователь заблокировал бота - выставить ему в таблице юзеров статус - IsBlocked.
        // Когда пользователь вновь написал боту (Users/CreateOrUpdate) - выставляем ему статус IsBlocked=false
        public async Task Notification(NotificationTask inputNotificationTask)
        {         
            string notificationTaskText = inputNotificationTask.ToCustomString();
            string notificationTaskLogMessage = $"Задача: {inputNotificationTask.Id} Рейс: {notificationTaskText}";

            try
            {
                // Задача помечается статусом "В работе"
                await _uow.NotificationTaskRepository.SetIsWorkedNotificationTask(inputNotificationTask);

                _logger.LogInformation($"Run {notificationTaskLogMessage} in Thread:{Thread.CurrentThread.ManagedThreadId}");

                //Во время выполнения задача стала неактуальна
                var notificationTaskIsActual = _uow.NotificationTaskRepository.NotificationTaskIsActual(inputNotificationTask);
                if (notificationTaskIsActual is false)
                {
                    await _uow.NotificationTaskRepository.SetIsNotActualAndIsNotWorked(inputNotificationTask);

                    string notActualTaskMessage = $"Stop {notificationTaskLogMessage} in Thread:{Thread.CurrentThread.ManagedThreadId}. Task has not actual.";
                    
                    _logger.LogInformation(notActualTaskMessage);
                    await _botApi.SendMessageForUserAsync(notActualTaskMessage,inputNotificationTask.UserId);

                    return;
                }
                
                //TODO: Во время выполнения задача была остановлена пользователем

                // Поиск свободных мест
                var resultFreeSeats = await _robot.GetFreeSeatsOnTheTrain(inputNotificationTask);

                //Если текущий результат равен предыдущему - завершаем задачу
                var resultIsLast = await _uow.NotificationTaskRepository.ResultIsLast(inputNotificationTask, resultFreeSeats);
                if (resultIsLast)
                {
                    await SetIsNotWorked(inputNotificationTask, notificationTaskLogMessage);
                    
                    return;
                }

                // Формирование текста уведомления о наличии мест
                string message ;

                // Если свободные места были, а сейчас их уже нет
                if (resultFreeSeats == "")
                    message = _robot.GetMessageSeatsIsEmpty(notificationTaskText);

                // Если свободных мест не было, а сейчас они появились
                // Или если изменилось количество свободных мест
                else
                    message = await _robot.GetMessageSeatsIsComplete(inputNotificationTask, resultFreeSeats);

                // Отправка сообщения пользователю
                await _botApi.SendMessageForUserAsync(message, inputNotificationTask.UserId);
                
                //TODO: Не нужно хранить всю строку, можно записывать н-р хэш
                // Пока оставил для удобства отладки. В будущем записывать хэш, или какую-то более краткую версию, например base64
                // которую можно расшифровать и она будет занимать меньше места чем исходная строка.
                // Записываем информацию о результате поиска свободных мест
                await _uow.NotificationTaskRepository.SetLastResultNotificationTask(inputNotificationTask, resultFreeSeats!);

                await SetIsNotWorked(inputNotificationTask, notificationTaskLogMessage);

                return;
            }
            catch (Exception e)
            {
                await SetIsNotWorked(inputNotificationTask, notificationTaskLogMessage);

                string messageError = $"Fatal Error. {notificationTaskLogMessage} {e}";
                _logger.LogError(messageError);
                await _botApi.SendMessageForAdminAsync(messageError);

                return;
            }
        }

        private async Task SetIsNotWorked(NotificationTask notificationTask, string logMessage)
        {
            await _uow.NotificationTaskRepository.SetIsNotWorked(notificationTask);

            _logger.LogInformation($"Stop {logMessage} in Thread:{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}