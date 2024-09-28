using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.EntityFrameworkCore.Repositories.StationInfos;
using RailwayWizzard.EntityFrameworkCore.Repositories.Users;

namespace RailwayWizzard.EntityFrameworkCore.UnitOfWork
{
    public class RailwayWizzardUnitOfWork : IRailwayWizzardUnitOfWork
    {
        public RailwayWizzardUnitOfWork(RailwayWizzardAppContext context)
        {
            _context = context;
            StationInfoRepository = new StationInfoRepository(context);
            NotificationTaskRepository = new NotificationTaskRepository(context);
            UserRepository = new UserRepository(context);
        }

        public IStationInfoRepository StationInfoRepository { get; }
        public INotificationTaskRepository NotificationTaskRepository { get; }

        public IUserRepository UserRepository { get; }

        private readonly RailwayWizzardAppContext _context;
    }
}
