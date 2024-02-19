﻿using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Data;


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
            builder.Services.AddDbContextFactory<RailwayWizzardAppContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("RailwayWizzardAppContext") 
                                  ?? throw new InvalidOperationException("Connection string 'RailwayWizzardAppContext' not found.")));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddHostedService<Worker>();

            var app = builder.Build();

            //Applying migrations to run programm
            var factory = app.Services.GetRequiredService<IDbContextFactory<RailwayWizzardAppContext>>();
            using(var context = factory.CreateDbContext())
                context.Database.Migrate();
            Console.WriteLine("Done applying migrations");

            // Configure the HTTP request pipeline.

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
