using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;


//RailwayWizzard\RailwayWizzard.App> dotnet ef migrations add test --project ..\RailwayWizzard.EntityFramework\
namespace RailwayWizzard.EntityFrameworkCore
{
    public class RailwayWizzardAppContext : DbContext
    {
        static RailwayWizzardAppContext()
        {
            //TODO:Кажется из-за этого в базу время записывается -3 hour
            //todo: Это решает проблему "System.InvalidCastException: Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported. "
            //todo: пока оставлю так, оно работает и это сейчас главнее
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

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
