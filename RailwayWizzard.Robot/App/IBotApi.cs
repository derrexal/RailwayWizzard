namespace RailwayWizzard.Robot.App;

/// <summary>
/// Описывает методы взаимодействия с ботом
/// </summary>
public interface IBotApi
{
    public Task SendMessageForUser(string message, long userId);
}