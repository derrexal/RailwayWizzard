using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayWizzard.Core.TrainBotTasks;

[Table("train_bot_tasks")]
public class TrainBotTaskEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("task_id")]
    public int TaskId { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; init; }

    [Column("telegram_id")]
    public long? TelegramId { get; init; }

    [Required]
    [Column("from_station")]
    public string From { get; init; } = string.Empty;

    [Required]
    [Column("to_station")]
    public string To { get; init; } = string.Empty;

    [Required]
    [Column("departure_date", TypeName = "date")]
    public DateOnly Date { get; init; }

    [Required]
    [Column("seats")]
    public short Seats { get; init; }

    [Required]
    [Column("mode")]
    public string Mode { get; init; } = string.Empty;

    [Required]
    [Column("seat_mode")]
    public string SeatMode { get; init; } = string.Empty;

    [Required]
    [Column("accessible_only")]
    public bool AccessibleOnly { get; init; }

    [Required]
    [Column("mode_params", TypeName = "jsonb")]
    public string ModeParams { get; init; } = "{}";

    [Required]
    [Column("seat_params", TypeName = "jsonb")]
    public string SeatParams { get; init; } = "{}";

    [Required]
    [Column("payload", TypeName = "jsonb")]
    public string Payload { get; init; } = "{}";

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
