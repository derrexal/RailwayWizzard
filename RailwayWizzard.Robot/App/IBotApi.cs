namespace RailwayWizzard.Robot.App;

/// <summary>
/// Описывает методы взаимодействия с ботом
/// </summary>
public interface IBotApi
{
    public Task SendMessageForUserAsync(string message, long userId);
    public Task SendMessageForAdminAsync(string message);

}