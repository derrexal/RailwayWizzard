using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Application.Services.B2B;
using RailwayWizzard.Application.Services.NotificationTasks;
using RailwayWizzard.Application.Services.Users;
using RailwayWizzard.Application.Workers;
using RailwayWizzard.Engine.Services;
using RailwayWizzard.Rzd.ApiClient.Services.GetFirewallTokenService;
using RailwayWizzard.Rzd.ApiClient.Services.GetStationDetailsService;
using RailwayWizzard.Rzd.ApiClient.Services.GetStationsByNameService;
using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService;
using RailwayWizzard.Infrastructure;
using RailwayWizzard.Infrastructure.Repositories.MessagesOutbox;
using RailwayWizzard.Infrastructure.Repositories.NotificationTaskResults;
using RailwayWizzard.Infrastructure.Repositories.NotificationTasks;
using RailwayWizzard.Infrastructure.Repositories.StationsInfo;
using RailwayWizzard.Infrastructure.Repositories.Users;
using RailwayWizzard.Rzd.DataEngine.Services;
using RailwayWizzard.Telegram.ApiClient.Services;

namespace RailwayWizzard.Application
{
    public abstract class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<IDataProcessor, DataProcessor>();
            builder.Services.AddScoped<IDataExtractor, DataExtractor>();

            builder.Services.AddTransient<IBotClient, BotClient>();

            builder.Services.AddTransient<IGetFirewallTokenService, GetFirewallTokenService>();
            builder.Services.AddTransient<IGetStationDetailsService, GetStationDetailsService>();
            builder.Services.AddTransient<IGetStationsByNameService, GetStationsByNameService>();
            builder.Services.AddTransient<IGetTrainInformationService, GetTrainInformationService>();

            builder.Services.AddScoped<IB2BService, B2BService>();
            builder.Services.AddScoped<INotificationTaskService, NotificationTaskService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<INotificationTaskRepository, NotificationTaskRepository>();
            builder.Services.AddScoped<INotificationTaskResultRepository, NotificationTaskResultRepository>();
            builder.Services.AddScoped<IMessageOutboxRepository, MessageOutboxRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IStationInfoRepository, StationInfoRepository>();
            
            builder.Configuration.AddEnvironmentVariables();

            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
                                   ?? throw new InvalidOperationException("DB_CONNECTION_STRING environment variable is not set.");
            
            builder.Services.AddDbContext<RailwayWizzardAppContext>(options =>
                options.UseNpgsql(connectionString,
                npgsqlOptionsAction: npgsqlOption =>
                    {
                        npgsqlOption.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null);
                    }
                ));
            
            builder.Services.AddControllers();

            builder.Services.AddHostedService<NotificationTaskWorker>();
            builder.Services.AddHostedService<MessageSenderWorker>();
            builder.Services.AddHostedService<HealthCheckWorker>();

            builder.Services.AddLogging(options =>
            {
                options.AddSimpleConsole(c =>
                {
                    c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                });
            });
            
            // builder.Services.AddHttpClient<IGetTrainInformationService, GetTrainInformationService>().AddPolicyHandler(GetRetryPolicy());
            builder.Services.AddHttpClient();

            var app = builder.Build();

            using (var serviceScope = app.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var dbContext = services.GetRequiredService<RailwayWizzardAppContext>();
                dbContext.Database.Migrate();

                // Before Run Program Update field IsWorked default value(false)
                dbContext.NotificationTasks.ExecuteUpdate(t =>
                    t.SetProperty(notificationTask => notificationTask.IsProcess, false));
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        
        // конечно ловит ошибки - у тебя при паре неудачных попыток последняя происходит спустя 5 в степени 3 = 625 секунд...
        // private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        // {
        //     return HttpPolicyExtensions
        //         .HandleTransientHttpError() // Ловит 5xx и сетевые ошибки
        //         .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(5, retryAttempt)));
        // }
    }
}
