namespace RailwayWizzard.Application.Options;

/// <summary>
/// Настройки подключения к RabbitMQ.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// Хост RabbitMQ.
    /// </summary>
    public string HostName { get; init; } = "rabbitmq";

    /// <summary>
    /// Порт RabbitMQ.
    /// </summary>
    public int Port { get; init; } = 5672;

    /// <summary>
    /// Имя пользователя RabbitMQ.
    /// </summary>
    public string UserName { get; init; } = "guest";

    /// <summary>
    /// Пароль RabbitMQ.
    /// </summary>
    public string Password { get; init; } = "guest";

    /// <summary>
    /// Виртуальный хост RabbitMQ.
    /// </summary>
    public string VirtualHost { get; init; } = "/";

    /// <summary>
    /// Очередь для чтения.
    /// </summary>
    public string InputQueue { get; init; } = "rzd_tasks";

    /// <summary>
    /// Очередь для отправки сообщений пользователям.
    /// </summary>
    public string MessagesQueue { get; init; } = "messages_to_user";

    /// <summary>
    /// Количество сообщений, получаемых за один раз.
    /// </summary>
    public ushort PrefetchCount { get; init; } = 50;

    /// <summary>
    /// Нужно ли объявлять очереди при запуске.
    /// </summary>
    public bool DeclareQueues { get; init; } = true;

    /// <summary>
    /// Делать ли очереди долговечными при объявлении.
    /// </summary>
    public bool DurableQueues { get; init; } = true;

    /// <summary>
    /// Нужно ли возвращать сообщение в очередь при ошибке обработки.
    /// </summary>
    public bool RequeueOnError { get; init; } = true;
}
