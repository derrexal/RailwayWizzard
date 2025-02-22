using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayWizzard.Core.MessageOutbox;

/// <summary>
/// Сущность сообщения для отправки пользователю.
/// </summary>
[Table("AppMessageOutbox")]
public class MessageOutbox : Entity
{
    /// <summary>
    /// Идентификатор задачи.
    /// </summary>
    public int NotificationTaskId { get; init; }
    
    /// <summary>
    /// Сообщение пользователю.
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// Время создания сообщения на отправку.
    /// </summary>
    public DateTime Created { get; set;}
    
    /// <summary>
    /// Сообщение отправлено пользователю.
    /// </summary>
    public bool IsSending { get; set; }
    
    /// <summary>
    /// Время отправки сообщения.
    /// </summary>
    public DateTime? Send { get; set;}
    
    /// <summary>
    /// Идентификатор пользователя, создавший эту задачу.
    /// </summary>
    public int UserId { get; init; }
}