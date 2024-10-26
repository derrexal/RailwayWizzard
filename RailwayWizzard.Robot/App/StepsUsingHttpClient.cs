﻿using Microsoft.Extensions.Logging;
using RailwayWizzard.Core;
using RailwayWizzard.Core.Shared;
using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using System.Diagnostics;

namespace RailwayWizzard.Robot.App
{
    /// <inheritdoc/>
    public class StepsUsingHttpClient : ISteps
    {
        private readonly IRobot _robot;
        private readonly IBotClient _botApi;
        private readonly INotificationTaskRepository _notificationTaskRepository;
        private readonly ILogger<StepsUsingHttpClient> _logger;

        private Stopwatch _watch;

        public StepsUsingHttpClient(
            IRobot robot,
            IBotClient botApi,
            INotificationTaskRepository notificationTaskRepository,
            ILogger<StepsUsingHttpClient> logger)
        {
            _robot = robot;
            _botApi = botApi;
            _notificationTaskRepository = notificationTaskRepository;
            _logger = logger;
            _watch = new Stopwatch(); // заглушка чтобы не ругался компилятор.
        }

        // TODO: сделать что-то с тем, что пользователи заблокировал бота...
        // Если выяснится что пользователь заблокировал бота - выставить ему в таблице юзеров статус - IsBlocked.
        // Когда пользователь вновь написал боту (Users/CreateOrUpdate) - выставляем ему статус IsBlocked=false
        public async Task Notification(NotificationTask notificationTask)
        {
            string notificationTaskLogMessage = $"Задача: {notificationTask.Id} Рейс: {notificationTask.ToCustomString()}";
            _watch = Stopwatch.StartNew();

            try
            {
                //TODO: Во время выполнения задача была остановлена пользователем
                
                //TODO: Такая ситуация вообще может произойти? Актуальность задачи проверяется на предыдущем шаге (воркер.)
                //Во время выполнения задача стала неактуальна
                if (notificationTask.IsActuality() == false)
                {
                    await _notificationTaskRepository.SetIsNotActualAndIsNotWorked(notificationTask);

                    //TODO: не вызываем реализованный метод  SetIsNotWorked !!! А может это вообще вырежем и не надо будет думат
                    string notActualTaskMessage = $"Stop {notificationTaskLogMessage} in Thread:{Thread.CurrentThread.ManagedThreadId}. Task has not actual.";

                    _logger.LogInformation(notActualTaskMessage);

                    return;
                }

                // Задача помечается статусом "В работе"
                await _notificationTaskRepository.SetIsWorkedNotificationTask(notificationTask);
                _logger.LogInformation($"Run {notificationTaskLogMessage} in Thread:{Thread.CurrentThread.ManagedThreadId}");

                // Поиск свободных мест
                var resultFreeSeats = await _robot.GetFreeSeatsOnTheTrain(notificationTask);

                //Если текущий результат равен предыдущему - завершаем задачу
                var resultIsLast = await _notificationTaskRepository.ResultIsLast(notificationTask, resultFreeSeats);
                if (resultIsLast)
                {
                    await SetIsNotWorked(notificationTask, notificationTaskLogMessage, "Result is last");

                    return;
                }

                // Текст сообщения пользователю
                string message;

                // Если свободные места были, а сейчас их уже нет
                if (resultFreeSeats == "")
                    message = _robot.GetMessageSeatsIsEmpty(notificationTask);

                // Если свободных мест не было, а сейчас они появились или изменилось количество свободных мест
                else
                    message = await _robot.GetMessageSeatsIsComplete(notificationTask, resultFreeSeats);

                // Отправка сообщения пользователю
                await _botApi.SendMessageForUserAsync(message, notificationTask.UserId);

                //TODO: Не нужно хранить всю строку, можно записывать н-р хэш
                // Пока оставил для удобства отладки. В будущем записывать хэш, или какую-то более краткую версию, например base64
                // которую можно расшифровать и она будет занимать меньше места чем исходная строка.
                // Записываем информацию о результате поиска свободных мест
                await _notificationTaskRepository.SetLastResultNotificationTask(notificationTask, resultFreeSeats!);

                await SetIsNotWorked(notificationTask, notificationTaskLogMessage, "Result is new");

                return;
            }
            catch (Exception e)
            {
                string messageError = $"Fatal Error. {notificationTaskLogMessage} {e}";
                _logger.LogError(messageError);
                await _botApi.SendMessageForAdminAsync(messageError);

                //TODO: логируем тут а потом ниже ?
                await SetIsNotWorked(notificationTask, notificationTaskLogMessage, "Fatal");

                return;
            }
        }

        private async Task SetIsNotWorked(NotificationTask notificationTask, string logMessage, string result)
        {
            await _notificationTaskRepository.SetIsNotWorked(notificationTask);

            _watch.Stop();

            _logger.LogInformation($"Stop {logMessage} in Thread:{Thread.CurrentThread.ManagedThreadId} Result:{result} Watch:{_watch.ElapsedMilliseconds}");
        }
    }
}