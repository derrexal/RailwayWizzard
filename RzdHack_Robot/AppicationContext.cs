using Microsoft.EntityFrameworkCore;
using RzdHack_Robot.Core;

public class ApplicationContext : DbContext
{
    public DbSet<NotificationTask> NotificationTasks => Set<NotificationTask>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=rzdhack_dev_stand;User id=postgres;Password=postgres;");
    }
}