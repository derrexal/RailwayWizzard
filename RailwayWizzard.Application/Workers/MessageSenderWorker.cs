using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RailwayWizzard.Application.Options;
using RailwayWizzard.Core.MessageOutbox;
using RailwayWizzard.Infrastructure.Repositories.MessagesOutbox;

namespace RailwayWizzard.Application.Workers
{
    public class MessageSenderWorker : BackgroundService
    {
        private const int RUN_INTERVAL = 1000 * 3; // 3 seconds
        
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MessageSenderWorker> _logger;
        private readonly RabbitMqOptions _options;

        private IConnection? _connection;
        private IModel? _channel;

        public MessageSenderWorker(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<MessageSenderWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            InitializeRabbitMq();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(RUN_INTERVAL, cancellationToken);
                await DoWork(cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var messageOutboxRepository = scope.ServiceProvider.GetRequiredService<IMessageOutboxRepository>();

            try
            {
                var messages = await messageOutboxRepository.GetNotSendMessagesAsync();

                foreach (var message in messages)
                {
                    PublishMessage(message);

                    await messageOutboxRepository.SetIsSendingAsync(message.Id);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"{nameof(MessageSenderWorker)} {ex}");
            }
        }

        private void PublishMessage(MessageOutbox message)
        {
            if (_channel is null)
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");

            var payload = new
            {
                task_id = message.NotificationTaskId,
                message = message.Message,
                created = message.Created,
                outbox_id = message.Id
            };

            var body = JsonSerializer.SerializeToUtf8Bytes(payload);
            var properties = _channel.CreateBasicProperties();
            properties.ContentType = "application/json";
            properties.ContentEncoding = "utf-8";
            properties.MessageId = message.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.DeliveryMode = _options.DurableQueues ? (byte)2 : (byte)1;

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _options.MessagesQueue,
                basicProperties: properties,
                body: body);
        }

        private void InitializeRabbitMq()
        {
            if (string.IsNullOrWhiteSpace(_options.MessagesQueue))
                throw new InvalidOperationException("RabbitMq:MessagesQueue is not configured.");

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            if (_options.DeclareQueues)
            {
                _channel.QueueDeclare(
                    queue: _options.MessagesQueue,
                    durable: _options.DurableQueues,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _connection?.Close();

            await base.StopAsync(cancellationToken);
        }
    }
}
