using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Services.B2B;
using RailwayWizzard.App.Services.NotificationTasks;
using RailwayWizzard.App.Services.Users;
using RailwayWizzard.B2B;
using RailwayWizzard.EntityFrameworkCore;
using RailwayWizzard.EntityFrameworkCore.Repositories.NotificationTasks;
using RailwayWizzard.EntityFrameworkCore.Repositories.StationInfos;
using RailwayWizzard.EntityFrameworkCore.Repositories.Users;
using RailwayWizzard.Robot.App;

namespace RailwayWizzard.App
{
    // TODO: отказаться от ABP
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddTransient<ISteps, StepsUsingHttpClient>();
            builder.Services.AddTransient<IRobot, RobotBigBrother>();
            builder.Services.AddTransient<IBotClient, BotClient>();

            builder.Services.AddTransient<IB2BClient, B2BClient>();

            builder.Services.AddTransient<IB2BService, B2BService>();
            builder.Services.AddTransient<INotificationTaskService, NotificationTaskService>();
            builder.Services.AddTransient<IUserService, UserService>();

            builder.Services.AddTransient<INotificationTaskRepository, NotificationTaskRepository>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<IStationInfoRepository, StationInfoRepository>();

            builder.Services.AddDbContext<RailwayWizzardAppContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("RailwayWizzardAppContext"),
                npgsqlOptionsAction: npgsqlOption =>
                    {
                        npgsqlOption.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    }));

            builder.Services.AddControllers();

            //builder.Services.AddHostedService<NotificationTaskWorker>();
            //builder.Services.AddHostedService<HealthCheckWorker>();

            builder.Services.AddLogging(options =>
            {
                options.AddSimpleConsole(c =>
                {
                    c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                });
            });

            builder.Services.AddHttpClient();

            var app = builder.Build();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
