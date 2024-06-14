using Microsoft.EntityFrameworkCore;
using RailwayWizzard.EntityFrameworkCore.Data;
using RailwayWizzard.Robot.App;
using RailwayWizzard.Shared;

namespace RailwayWizzard.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //TODO:Кажется из-за этого в базу время записывается -3 hour
            //todo: Это решает проблему "System.InvalidCastException: Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported. "
            //todo: пока оставлю так, оно работает и это сейчас главнее
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddTransient<IChecker, NotificationTaskChecker>();
            builder.Services.AddTransient<IBotApi, BotApi>();
            builder.Services.AddTransient<IRobot, RobotBigBrother>();
            builder.Services.AddTransient<ISteps, StepsUsingHttpClient>();

            builder.Services.AddDbContextFactory<RailwayWizzardAppContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("RailwayWizzardAppContext") 
                                  ?? throw new InvalidOperationException("Connection string 'RailwayWizzardAppContext' not found.")));

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddHostedService<NotificationTaskWorker>();
            builder.Services.AddHostedService<HealthCheckWorker>();
            builder.Services.AddHostedService<MosRuWorker>();

            builder.Services.AddLogging(options =>
            {
                options.AddSimpleConsole(c =>
                {
                    c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                });
            });
            
            var app = builder.Build();

            var factory = app.Services.GetRequiredService<IDbContextFactory<RailwayWizzardAppContext>>();
            using(var context = factory.CreateDbContext())
            {
                //Applying migrations to run programm
                context.Database.Migrate();
                //Before Run Program Update field IsWorked default value (false)
                context.NotificationTask.ExecuteUpdate(t =>
                    t.SetProperty(t => t.IsWorked, false));
            }

            // Configure the HTTP request pipeline.

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
            //TODO: Удалить неиспользуемые ссылки в проектах
        }
    }
}
