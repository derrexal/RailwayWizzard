using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayWizzard.Core.NotificationTaskResult;

/// <summary>
/// Сущность состояния выполненной задачи.
/// </summary>
[Table("AppNotificationTaskResult")]
public class NotificationTaskResult : Entity
{
    /// <summary>
    /// Идентификатор задачи.
    /// </summary>
    public int NotificationTaskId { get; init; }
    
    /// <summary>
    /// Время старта задачи.
    /// </summary>
    public DateTime Started { get; init; }
    
    /// <summary>
    /// Время завершения задачи.
    /// </summary>
    public DateTime Finished { get; init; }
    
    /// <summary>
    /// Статус задачи.
    /// </summary>
    public NotificationTaskResultStatus ResultStatus { get; init; }
    
    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    public string? Error  { get; init; }
    
    /// <summary>
    /// Хэш результата.
    /// </summary>
    [MaxLength(32)]
    public byte[]? HashResult { get; init; }
}