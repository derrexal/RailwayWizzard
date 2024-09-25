using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.EntityFrameworkCore.Repositories.StationInfos;

namespace RailwayWizzard.EntityFrameworkCore.UnitOfWork
{
    public class RailwayWizzardUnitOfWork : IRailwayWizzardUnitOfWork
    {
        private readonly RailwayWizzardAppContext _context;

        public RailwayWizzardUnitOfWork(RailwayWizzardAppContext context)
        {
            _context = context;
            StationInfoRepository = new StationInfoRepository(context);
            NotificationTaskRepository = new NotificationTaskRepository(context);
        }

        public IStationInfoRepository StationInfoRepository { get; }
        public INotificationTaskRepository NotificationTaskRepository { get; }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
