using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.EntityFrameworkCore.Repositories.StationInfos;

namespace RailwayWizzard.EntityFrameworkCore.UnitOfWork
{
    public interface IRailwayWizzardUnitOfWork : IDisposable
    {
        IStationInfoRepository StationInfoRepository { get; }

        INotificationTaskRepository NotificationTaskRepository { get; }
    }
}
