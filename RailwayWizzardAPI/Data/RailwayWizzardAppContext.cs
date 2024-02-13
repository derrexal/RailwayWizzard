using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RzdHack_Robot.Core;

namespace RailwayWizzard.App.Data
{
    public class RailwayWizzardAppContext : DbContext
    {
        public RailwayWizzardAppContext (DbContextOptions<RailwayWizzardAppContext> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<StationInfo> StationInfo { get; set; }
        public DbSet<NotificationTask> NotificationTask { get; set; }
    }
}
