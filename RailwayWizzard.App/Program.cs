using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Services;
using RailwayWizzard.App.Services.Shared;
using RailwayWizzard.B2B;
using RailwayWizzard.EntityFrameworkCore;
using RailwayWizzard.EntityFrameworkCore.UnitOfWork;
using RailwayWizzard.Robot.App;

namespace RailwayWizzard.App
{
    // TODO: отказаться от ABP
    public class Program
    {
        public static void Main(string[] args)
        {
            //TODO:Кажется из-за этого в базу время записывается -3 hour
            //todo: Это решает проблему "System.InvalidCastException: Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported. "
            //todo: пока оставлю так, оно работает и это сейчас главнее
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddTransient<IBotApi, BotApi>();
            builder.Services.AddTransient<IRobot, RobotBigBrother>();
            builder.Services.AddTransient<ISteps, StepsUsingHttpClient>();
            builder.Services.AddTransient<IB2BClient, B2BClient>();

            builder.Services.AddScoped<IB2BService, B2BService>();
            builder.Services.AddScoped<INotificationTaskService, NotificationTaskService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IRailwayWizzardUnitOfWork,RailwayWizzardUnitOfWork>();

            builder.Services.AddDbContextFactory<RailwayWizzardAppContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("RailwayWizzardAppContext") 
                                  ?? throw new InvalidOperationException("Connection string 'RailwayWizzardAppContext' not found.")));

            builder.Services.AddControllers();

            builder.Services.AddHostedService<NotificationTaskWorker>();
            builder.Services.AddHostedService<HealthCheckWorker>();

            builder.Services.AddLogging(options =>
            {
                options.AddSimpleConsole(c =>
                {
                    c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                });
            });

            builder.Services.AddHttpClient();

            var app = builder.Build();

            var factory = app.Services.GetRequiredService<IDbContextFactory<RailwayWizzardAppContext>>();
            using(var context = factory.CreateDbContext())
            {
                //Applying migrations to run program
                context.Database.Migrate();
                
                //Before Run Program Update field IsWorked default value (false)
                context.NotificationTask.ExecuteUpdate(t =>
                    t.SetProperty(t => t.IsWorked, false));
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
