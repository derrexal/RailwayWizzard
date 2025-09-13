using RailwayWizzard.Infrastructure.Repositories.MessagesOutbox;
using RailwayWizzard.Infrastructure.Repositories.Users;
using RailwayWizzard.Telegram.ApiClient.Exceptions;
using RailwayWizzard.Telegram.ApiClient.Models;
using RailwayWizzard.Telegram.ApiClient.Services;

namespace RailwayWizzard.Application.Workers
{
    // TODO: добавить обработку следующих лимитов:
    // - 1 сообщение в 1 секунду в 1 чат
    // - 30 сообщений в секунду в 30 разных чатов
    // - 1 сообщение - 512 байт
    public class MessageSenderWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 3; // 3 second
        
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NotificationTaskWorker> _logger;

        public MessageSenderWorker(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<NotificationTaskWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //_logger.LogInformation($"{nameof(MessageSenderWorker)} running at: {DateTimeExtensions.MoscowNow} Moscow time");
                await Task.Delay(RUN_INTERVAL, cancellationToken);
                await DoWork(cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken) // TODO: прокинуть cancellationToken
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var messageOutboxRepository = scope.ServiceProvider.GetRequiredService<IMessageOutboxRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var botClient = scope.ServiceProvider.GetRequiredService<IBotClient>();

            try
            {
                var messages = await messageOutboxRepository.GetNotSendMessagesAsync();

                foreach (var message in messages)
                {
                    var user = await userRepository.GetUserByIdAsync(message.UserId);

                    var sendMessageDto = new MessageDto(message.Message, user.TelegramUserId);

                    try
                    {
                        await botClient.SendMessageForUserAsync(sendMessageDto);
                    }

                    catch (ConflictHttpException)
                    {
                        await userRepository.SetHasBlockedBotAsync(message.UserId);
                    }

                    await messageOutboxRepository.SetIsSendingAsync(message.Id);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"{nameof(MessageSenderWorker)} {ex}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"{nameof(MessageSenderWorker)} stopped at: {DateTimeExtensions.MoscowNow} Moscow time");

            await base.StopAsync(cancellationToken);
        }
    }
}