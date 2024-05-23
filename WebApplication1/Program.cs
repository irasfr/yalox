using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Register database context
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(""));

            // Register IDbService with DbService implementation
            builder.Services.AddScoped<IDbService, DbService>();

            // Register EventScheduler
            builder.Services.AddScoped<IEventScheduler, EventScheduler>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            var eventScheduler = app.Services.GetRequiredService<IEventScheduler>();
            var timer = new System.Threading.Timer(async _ => await eventScheduler.TrySchedule(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            app.Run();
        }
    }
}