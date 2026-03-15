using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayWizzard.Core.TrainBotTasks;

public static class TrainBotTaskStatusValues
{
    public const string Processing = "processing";
    public const string Error = "error";
    public const string End = "end";
}

[Table("train_bot_task_statuses")]
public class TrainBotTaskStatus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("task_id")]
    public int TaskId { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = TrainBotTaskStatusValues.Processing;

    [Column("last_error")]
    public string? LastError { get; set; }

    [ForeignKey(nameof(TaskId))]
    public TrainBotTaskEntity? Task { get; set; }
}
