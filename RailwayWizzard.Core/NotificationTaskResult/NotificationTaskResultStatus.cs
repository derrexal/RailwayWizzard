namespace RailwayWizzard.Core.NotificationTaskResult;

/// <summary>
/// Статус задачи.
/// </summary>
public enum NotificationTaskResultStatus
{
    /// <summary>
    /// Текущий результат.
    /// </summary>
    Current = 1,

    /// <summary>
    /// Новый результат.
    /// </summary>
    New = 2,
    
    /// <summary>
    /// Ошибка при выполнении задачи.
    /// </summary>
    Error = 3,
}