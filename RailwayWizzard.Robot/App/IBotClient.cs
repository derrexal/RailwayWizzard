namespace RailwayWizzard.Robot.App;


/// <summary>
/// Описывает методы взаимодействия с ботом
/// </summary>
public interface IBotClient
{
    /// <summary>
    /// Метод отправки сообщения пользователю
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="userId">Id пользователя</param>
    /// <returns>Задача</returns>
    public Task SendMessageForUserAsync(string message, long userId);

    /// <summary>
    /// Метод отправки сообщения администратору
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <returns>Задача</returns>
    public Task SendMessageForAdminAsync(string message);

}