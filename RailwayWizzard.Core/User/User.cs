using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayWizzard.Core.User;

[Table("AppUsers")]
public class User : Entity
{
    /// <summary>
    /// Идентификатор пользователя Telegram.
    /// </summary>
    [Required]
    public long TelegramUserId { get; set; }

    /// <summary>
    /// Никнейм пользователя.
    /// Telegram позволяет скрывать никнейм.
    /// </summary>
    [MaxLength(32)]
    public string? Username { get; set; }

    /// <summary>
    /// Указывает, заблокировал ли пользователь бота.
    /// </summary>
    public bool HasBlockedBot { get; set; }
}