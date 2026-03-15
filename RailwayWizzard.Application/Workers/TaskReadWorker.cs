using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RailwayWizzard.Application.Options;
using RailwayWizzard.Core.TrainBotOutbox;
using RailwayWizzard.Infrastructure.Repositories.TrainBotOutbox;

namespace RailwayWizzard.Application.Workers
{
    /// <summary>
    /// Воркер для чтения сообщений из RabbitMQ и сохранения в БД.
    /// </summary>
    public class TaskReadWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TaskReadWorker> _logger;
        private readonly RabbitMqOptions _options;
        private readonly SemaphoreSlim _processingLock = new(1, 1);

        private IConnection? _connection;
        private IModel? _consumeChannel;

        public TaskReadWorker(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<TaskReadWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            ValidateOptions();
            InitializeRabbitMq();

            _logger.LogInformation(
                "{Worker} started. InputQueue={InputQueue}",
                nameof(TaskReadWorker),
                _options.InputQueue);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_consumeChannel is null)
                throw new InvalidOperationException("RabbitMQ consume channel is not initialized.");

            var consumer = new AsyncEventingBasicConsumer(_consumeChannel);
            consumer.Received += OnMessageReceivedAsync;

            _consumeChannel.BasicConsume(
                queue: _options.InputQueue,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _consumeChannel?.Close();
            _connection?.Close();

            await base.StopAsync(cancellationToken);
        }

        private Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
        {
            return HandleMessageAsync(args);
        }

        private async Task HandleMessageAsync(BasicDeliverEventArgs args)
        {
            await _processingLock.WaitAsync();

            try
            {
                var payload = Encoding.UTF8.GetString(args.Body.ToArray());

                using var scope = _serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITrainBotOutboxRepository>();

                var message = new TrainBotMessage
                {
                    QueueName = _options.InputQueue,
                    Payload = payload,
                    Status = "NEW",
                    Attempts = 0,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var messageId = TryGetMessageId(args);
                if (messageId.HasValue)
                    message.Id = messageId.Value;

                await repository.CreateAsync(message);

                if (_consumeChannel?.IsOpen == true)
                    _consumeChannel.BasicAck(args.DeliveryTag, multiple: false);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logger.LogWarning(ex,
                    "{Worker} received duplicate message for queue {Queue}",
                    nameof(TaskReadWorker),
                    _options.InputQueue);

                if (_consumeChannel?.IsOpen == true)
                    _consumeChannel.BasicAck(args.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{Worker} failed to process message from queue {Queue}",
                    nameof(TaskReadWorker),
                    _options.InputQueue);

                if (_consumeChannel?.IsOpen == true)
                    _consumeChannel.BasicNack(args.DeliveryTag, multiple: false, requeue: _options.RequeueOnError);
            }
            finally
            {
                _processingLock.Release();
            }
        }

        private void ValidateOptions()
        {
            if (string.IsNullOrWhiteSpace(_options.InputQueue))
                throw new InvalidOperationException("RabbitMq:InputQueue is not configured.");
        }

        private void InitializeRabbitMq()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _consumeChannel = _connection.CreateModel();

            if (_options.DeclareQueues)
            {
                _consumeChannel.QueueDeclare(
                    queue: _options.InputQueue,
                    durable: _options.DurableQueues,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }

            _consumeChannel.BasicQos(0, _options.PrefetchCount, global: false);
        }

        private static long? TryGetMessageId(BasicDeliverEventArgs args)
        {
            var messageId = args.BasicProperties?.MessageId;
            if (!string.IsNullOrWhiteSpace(messageId) && long.TryParse(messageId, out var parsed))
                return parsed;

            if (args.BasicProperties?.Headers is null)
                return null;

            if (!args.BasicProperties.Headers.TryGetValue("outbox_id", out var headerValue))
                return null;

            return headerValue switch
            {
                long longValue => longValue,
                int intValue => intValue,
                string strValue when long.TryParse(strValue, out var strParsed) => strParsed,
                byte[] bytes when long.TryParse(Encoding.UTF8.GetString(bytes), out var byteParsed) => byteParsed,
                _ => null
            };
        }

        private static bool IsUniqueViolation(DbUpdateException exception)
        {
            return exception.InnerException is PostgresException pg &&
                   pg.SqlState == PostgresErrorCodes.UniqueViolation;
        }
    }
}
