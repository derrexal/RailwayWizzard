using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.EntityFrameworkCore.Repositories.StationInfos;
using RailwayWizzard.EntityFrameworkCore.Repositories.Users;

namespace RailwayWizzard.EntityFrameworkCore.UnitOfWork
{
    public interface IRailwayWizzardUnitOfWork : IDisposable
    {
        IStationInfoRepository StationInfoRepository { get; }

        INotificationTaskRepository NotificationTaskRepository { get; }

        IUserRepository UserRepository { get; }
    }
}
