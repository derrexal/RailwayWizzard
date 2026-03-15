using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayWizzard.Core.TrainBotOutbox;

/// <summary>
/// Сообщение из outbox сервиса train_bot.
/// </summary>
[Table("outbox")] //TODO: rename to inbox_train_info
public class TrainBotMessage
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("queue_name")]
    public required string QueueName { get; init; }

    [Required]
    [Column("payload", TypeName = "jsonb")]
    public required string Payload { get; init; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = "NEW";

    [Required]
    [Column("attempts")]
    public int Attempts { get; set; } = 0;

    [Column("last_error")]
    public string? LastError { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("locked_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset? LockedAt { get; set; }

    [Column("sent_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset? SentAt { get; set; }
}
