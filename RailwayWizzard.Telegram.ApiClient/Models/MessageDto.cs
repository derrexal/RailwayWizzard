namespace RailwayWizzard.Telegram.ApiClient.Models;

/// <summary>
/// Модель сообщения пользователю.
/// </summary>
public record MessageDto(string Message, long TelegramUserId);
