using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;

//RailwayWizzard\RailwayWizzard.App> dotnet ef migrations add test --project ..\RailwayWizzard.EntityFramework\
namespace RailwayWizzard.EntityFrameworkCore
{
    public class RailwayWizzardAppContext : DbContext
    {
        static RailwayWizzardAppContext() => 
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        public RailwayWizzardAppContext(DbContextOptions<RailwayWizzardAppContext> options) : base(options) { }

        public DbSet<User> User { get; set; }

        public DbSet<StationInfo> StationInfo { get; set; }
        
        public DbSet<NotificationTask> NotificationTask { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StationInfo>()
                .HasIndex(s => s.ExpressCode)
                .IsUnique();
        }
    }
}
