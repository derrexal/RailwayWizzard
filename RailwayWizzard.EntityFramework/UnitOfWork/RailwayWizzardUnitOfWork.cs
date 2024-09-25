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

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
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
