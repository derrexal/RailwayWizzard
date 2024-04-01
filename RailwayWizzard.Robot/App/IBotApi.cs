namespace RailwayWizzard.Robot.App;

public interface IBotApi
{
    public Task SendMessageForUser(string message, long userId);
}