using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;

// dotnet ef migrations add test --project ..\RailwayWizzard.EntityFramework\
namespace RailwayWizzard.EntityFrameworkCore.Data
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
