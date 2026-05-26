
using AE.Market.Application;
using AE.Market.Infrastructure;
using AE.Market.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using System.Buffers.Text;
namespace AE.Market.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));


            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Add Services from other layers
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            var app = builder.Build();

            // Auto-apply migrations in development
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
                app.MapOpenApi();
                app.MapScalarApiReference();
                var httpUrl = app.Urls.FirstOrDefault(u => u.StartsWith("http://")) ?? "http://localhost:8080";
                var apiUrl = httpUrl.Replace("://*:", "://localhost:").Replace("://+:", "://localhost:");
                Console.WriteLine("═══════════════════════════════════════════");
                Console.WriteLine($"  API:           {apiUrl}");
                Console.WriteLine($"  Scalar docs:   {apiUrl}/scalar");
                Console.WriteLine($"  Seq dashboard: http://localhost:8082");
                Console.WriteLine("═══════════════════════════════════════════");
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
