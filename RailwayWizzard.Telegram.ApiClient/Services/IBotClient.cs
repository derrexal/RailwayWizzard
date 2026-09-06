using RailwayWizzard.Telegram.ApiClient.Models;

namespace RailwayWizzard.Telegram.ApiClient.Services;

/// <summary>
/// Сервис взаимодействия с телеграм ботом.
/// </summary>
public interface IBotClient
{
    /// <summary>
    /// Метод отправки сообщения пользователю.
    /// </summary>
    /// <param name="messageDto">Модель сообщения пользователю.</param>
    /// <returns>Задача <see cref="Task"/>.</returns>
    public Task SendMessageForUserAsync(MessageDto messageDto);
}