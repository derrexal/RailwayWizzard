using Abp.Dependency;

namespace RailwayWizzard.Robot.App;

public interface IBotApi: ITransientDependency
{
    public Task SendMessageForUser(string message, long userId);
}