using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.EntityFrameworkCore.Repositories.StationInfos;

namespace RailwayWizzard.EntityFrameworkCore.UnitOfWork
{
    public class RailwayWizzardUnitOfWork : IRailwayWizzardUnitOfWork
    {
        //TODO: вынести в DI?
        public RailwayWizzardUnitOfWork(RailwayWizzardAppContext context)
        {
            StationInfoRepository = new StationInfoRepository(context);
            NotificationTaskRepository = new NotificationTaskRepository(context);
        }

        public IStationInfoRepository StationInfoRepository { get; }
        public INotificationTaskRepository NotificationTaskRepository { get; }

    }
}
