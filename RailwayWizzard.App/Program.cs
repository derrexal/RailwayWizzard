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
            builder.Services.AddScoped<IRobot, RobotBigBrother>();
            builder.Services.AddScoped<IBotApi, BotApi>();

            builder.Services.AddScoped<IB2BClient, B2BClient>();

            builder.Services.AddScoped<IB2BService, B2BService>();
            builder.Services.AddScoped<INotificationTaskService, NotificationTaskService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<INotificationTaskRepository, NotificationTaskRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IStationInfoRepository, StationInfoRepository>();

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

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
