using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core.MessageOutbox;
using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Core.NotificationTaskResult;
using RailwayWizzard.Core.StationInfo;
using RailwayWizzard.Core.User;

//RailwayWizzard\RailwayWizzard.Application> dotnet ef migrations add test --project ..\RailwayWizzard.Infrastructure\
namespace RailwayWizzard.Infrastructure
{
    public class RailwayWizzardAppContext : DbContext
    {
        static RailwayWizzardAppContext() => 
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        public RailwayWizzardAppContext(DbContextOptions<RailwayWizzardAppContext> options) : base(options) { }

        public DbSet<User> Users { get; init; }

        [Obsolete("Удалить после успешного переезда на новую таблицу + переименовать старую таблицу в old + переименовать новую сущность")]
        public DbSet<StationInfo> StationsInfo { get; init; }
        
        public DbSet<StationInfoExtended> StationsInfoExtended { get; init; }

        public DbSet<NotificationTask> NotificationTasks { get; init; }
        
        public DbSet<NotificationTaskResult> NotificationTasksProcess { get; init; }

        public DbSet<MessageOutbox> Messages { get; init; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StationInfoExtended>()
                .HasIndex(s => s.ExpressCode);
        }
    }
}
