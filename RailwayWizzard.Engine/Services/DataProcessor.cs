using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RailwayWizzard.Common;
using RailwayWizzard.Core.MessageOutbox;
using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Core.NotificationTaskResult;
using RailwayWizzard.Infrastructure.Repositories.MessagesOutbox;
using RailwayWizzard.Infrastructure.Repositories.NotificationTaskResults;
using RailwayWizzard.Infrastructure.Repositories.NotificationTasks;
using RailwayWizzard.Infrastructure.Repositories.StationsInfo;
using RailwayWizzard.Rzd.DataEngine.Services;
using static System.Threading.Thread;

namespace RailwayWizzard.Engine.Services
{
    /// <inheritdoc/>
    public class DataProcessor : IDataProcessor
    {
        private readonly IDataExtractor _dataExtractor;
        private readonly INotificationTaskRepository _taskRepository;
        private readonly INotificationTaskResultRepository _taskResultRepository;
        private readonly IMessageOutboxRepository _messageOutboxRepository;
        private readonly IStationInfoRepository _stationInfoRepository;
        private readonly ILogger<DataProcessor> _logger;

        private readonly Stopwatch _watch;
        private readonly DateTime _started;
        
        public DataProcessor(
            IDataExtractor dataExtractor,
            INotificationTaskRepository taskRepository,
            INotificationTaskResultRepository taskResultRepository,
            IMessageOutboxRepository messageOutboxRepository,
            ILogger<DataProcessor> logger, IStationInfoRepository stationInfoRepository)
        {
            _dataExtractor = dataExtractor;
            _taskRepository = taskRepository;
            _taskResultRepository = taskResultRepository;
            _messageOutboxRepository = messageOutboxRepository;
            _logger = logger;
            _stationInfoRepository = stationInfoRepository;
            _watch = Stopwatch.StartNew();
            _started = DateTime.Now;
        }

        public async Task RunProcessTaskAsync(NotificationTask task)
        {
            var logMessage = $"Task ID: {task.Id} Details: {task.ToLogString()}";

            _logger.LogInformation($"Run {logMessage} in Thread:{Environment.CurrentManagedThreadId}");

            try
            {
                // Получаем ответ от РЖД
                await _taskRepository.SetIsProcessAsync(task.Id);

                var freeSeatsResult = await _dataExtractor.FindFreeSeatsAsync(task);
                
                var hashResult = freeSeatsResult.ToSha256Hash();
                var lastTaskProcess = await _taskResultRepository.GetLastNotificationTaskProcessAsync(task.Id);
                if (lastTaskProcess?.HashResult != null && hashResult.SequenceEqual(lastTaskProcess.HashResult))
                {
                    await CreateNotificationTaskResult(task.Id, NotificationTaskResultStatus.Current, hashResult);
                    await SetIsNotWorkedAsync(task.Id, logMessage, NotificationTaskResultStatus.Current);
                    return;
                }

                // Формируем сообщение пользователю
                string textMessage;
                if (freeSeatsResult == "")
                    textMessage = await GetMessageSeatsIsEmptyAsync(task);
                else
                    textMessage = await GetMessageSeatsIsCompleteAsync(task, freeSeatsResult);

                await CreateMessage(task.Id, textMessage, task.CreatorId);
                await CreateNotificationTaskResult(task.Id, NotificationTaskResultStatus.New, hashResult);
                await SetIsNotWorkedAsync(task.Id, logMessage, NotificationTaskResultStatus.New);
            }
            
            catch (Exception e)
            {
                var errorMessage = $"Fatal Error. {logMessage} {e.Message}";
                
                // Временно отключил отправку ошибок администратору - так как
                // это приводит к накоплению многочисленных идентичных сообщений об ошибках
                // из-за этого в очередь попадает куча сообщений
                // которые нужно "протолкнуть" через узкое горлышко телергам бота (ограничения количество отправленных сообщений)
                // await CreateMessage(task.Id, errorMessage, BussinesConstants.ADMIN_USER_ID);
                await CreateNotificationTaskResult(task.Id, NotificationTaskResultStatus.Error, null, errorMessage);
                await SetIsNotWorkedAsync(task.Id, logMessage, NotificationTaskResultStatus.Error);
                
                _logger.LogError($"Stop {logMessage} in Thread:{Environment.CurrentManagedThreadId} " + 
                                 $"Result: Fatal Error Watch:{_watch.ElapsedMilliseconds} Details Error: \n{e}");
            }
        }

        /// <summary>
        /// Формирует сообщение пользователю на случай
        /// если свободные места были, а сейчас их уже нет.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>Сообщение.</returns>
        private async Task<string> GetMessageSeatsIsEmptyAsync(NotificationTask task)
        {
            var departureStation = await _stationInfoRepository.GetByIdAsync(task.DepartureStationId);
            var arrivalStation = await _stationInfoRepository.GetByIdAsync(task.ArrivalStationId);
            
            return $"{char.ConvertFromUtf32(0x26D4)} " +
                   $"{task.ToBotString(departureStation.Name, arrivalStation.Name)}" +
                   "\nСвободных мест больше нет";
        }
        
        /// <summary>
        /// Формирует сообщение пользователю на случай  если свободных мест не было, а сейчас они появились
        /// или изменилось количество свободных мест
        /// </summary>
        /// <param name="task"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task<string> GetMessageSeatsIsCompleteAsync(NotificationTask task, string result)
        {
            var linkToBuyTicket = await _dataExtractor.GetLinkToBuyTicketAsync(task);
            
            var linkToBuyTicketFormat = linkToBuyTicket is not null
                ? $"\n<a href=\"{linkToBuyTicket}\">Купить билет</a>"
                : "";

            var departureStation = await _stationInfoRepository.GetByIdAsync(task.DepartureStationId);
            var arrivalStation = await _stationInfoRepository.GetByIdAsync(task.ArrivalStationId);
            
            return
                $"{char.ConvertFromUtf32(0x2705)} {task.ToBotString(departureStation.Name, arrivalStation.Name)}" +
                $"\n\n{result}" +
                linkToBuyTicketFormat;
        }

        
        private async Task SetIsNotWorkedAsync(int taskId, string message, NotificationTaskResultStatus result)
        {
            // Выглядит странно :)
            await _taskRepository.SetIsNotWorkedAsync(taskId);

            await _taskRepository.SetIsUpdatedAsync(taskId);

            _watch.Stop();
            
            if(result is NotificationTaskResultStatus.Error)
                return;
            
            _logger.LogInformation(
                $"Stop {message} in Thread:{CurrentThread.ManagedThreadId} " +
                $"Result:{result.ToString()} Watch:{_watch.ElapsedMilliseconds}");
        }

        private async Task CreateMessage(int taskId, string textMessage, int userId)
        {
            var message = new MessageOutbox
            {
                NotificationTaskId = taskId,
                Message = textMessage,
                Created = DateTime.Now,
                UserId = userId
            };
            await _messageOutboxRepository.CreateAsync(message);
        }

        private async Task CreateNotificationTaskResult(int taskId, NotificationTaskResultStatus status, byte[]? hashResult = null, string? errorMessage = null)
        {
            var taskProcess = new NotificationTaskResult
            {
                NotificationTaskId = taskId,
                Started = _started,
                Finished = DateTime.Now,
                ResultStatus = status,
                HashResult = hashResult,
                Error =  errorMessage,
            };
                
            await _taskResultRepository.CreateAsync(taskProcess);
        }
    }
}