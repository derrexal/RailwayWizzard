using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RailwayWizzard.App.Controllers;
using RailwayWizzard.App.Data;
namespace RailwayWizzardAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<RailwayWizzardAppContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("RailwayWizzardAppContext") ?? throw new InvalidOperationException("Connection string 'RailwayWizzardAppContext' not found.")));

            // Add services to the container.

            builder.Services.AddControllers();
            var app = builder.Build();

            //TODO:Кажется из-за этого в базу время записывается -3 hour
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
