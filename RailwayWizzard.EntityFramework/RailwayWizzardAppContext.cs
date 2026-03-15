using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core.MessageOutbox;
using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Core.NotificationTaskResult;
using RailwayWizzard.Core.StationInfo;
using RailwayWizzard.Core.TrainBotOutbox;
using RailwayWizzard.Core.TrainBotTasks;

//RailwayWizzard\RailwayWizzard.Application> dotnet ef migrations add test --project ..\RailwayWizzard.Infrastructure\
namespace RailwayWizzard.Infrastructure
{
    public class RailwayWizzardAppContext : DbContext
    {
        static RailwayWizzardAppContext() => 
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        public RailwayWizzardAppContext(DbContextOptions<RailwayWizzardAppContext> options) : base(options) { }

        public DbSet<StationInfo> StationsInfo { get; init; }
        
        public DbSet<NotificationTask> NotificationTasks { get; init; }
        
        public DbSet<NotificationTaskResult> NotificationTasksProcess { get; init; }

        public DbSet<MessageOutbox> Messages { get; init; }

        public DbSet<TrainBotMessage> TrainBotOutboxMessages { get; init; }
        
        public DbSet<TrainBotTaskEntity> TrainBotTasks { get; init; }

        public DbSet<TrainBotTaskStatus> TrainBotTaskStatuses { get; init; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StationInfo>()
                .HasIndex(s => s.ExpressCode)
                .IsUnique();

            modelBuilder.Entity<MessageOutbox>()
                .HasIndex(m => new { m.NotificationTaskId, m.Message })
                .IsUnique()
                .HasDatabaseName("IX_AppMessageOutbox_NotificationTaskId_Message");

            modelBuilder.Entity<TrainBotTaskStatus>()
                .ToTable(t => t.HasCheckConstraint(
                    "ck_train_bot_task_status_status_allowed",
                    "status IN ('processing','error','end')"));
        }
    }
}
