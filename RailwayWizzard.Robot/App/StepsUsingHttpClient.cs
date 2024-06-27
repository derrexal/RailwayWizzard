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
        private readonly ILogger _logger;
        private readonly IDbContextFactory<RailwayWizzardAppContext> _contextFactory;
        
        public StepsUsingHttpClient(
            IRobot robot,
            IBotApi botApi,
            IChecker checker,
            ILogger logger, 
            IDbContextFactory<RailwayWizzardAppContext> contextFactory)
        {
            _robot = robot;
            _botApi = botApi;
            _checker = checker;
            _logger = logger;
            _contextFactory = contextFactory;
        }

        // TODO: сделать что-то с тем, что пользователи заблокировал бота...
        // Если выяснится что пользователь заблокировал бота - выставить ему в таблице юзеров статус - IsBlocked.
        // Когда пользователь вновь написал боту (Users/CreateOrUpdate) - выставляем ему статус IsBlocked=false
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
                            currentNotificationTask!.IsWorked = false;
                            await context.SaveChangesAsync();
                            _logger.LogTrace($"Во время выполнения программы задача {inputNotificationTask.Id} " +
                                             $"была остановлена пользователем. Подробности задачи:{railwayDataText}");
                            break;
                        }
                    }

                    // Формирование текста уведомления о наличии мест
                    string message = $"{char.ConvertFromUtf32(0x2705)} {railwayDataText}" +
                                     $"\n{String.Join("\n", freeSeats.ToArray())}" +
                                     "\nОбнаружены свободные места\n";
                    
                    // Отправка сообщения пользователю
                    await _botApi.SendMessageForUserAsync(message, inputNotificationTask.UserId);
                                            
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

                _logger.LogError($"Неизвестная ошибка метода обработки задач. {messageNotification}\n {e}");
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
            List<string> result;
            do
            {
                Thread.Sleep(1000 * 30); //пол минуты
                result = await _robot.GetFreeSeatsOnTheTrain(inputNotificationTask);
            }
            while (result.Count==0);

            return result;
        }
    }
}
