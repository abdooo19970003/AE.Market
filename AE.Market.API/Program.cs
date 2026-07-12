using AE.Market.API.Authentication;
using AE.Market.API.Exceptions;
using AE.Market.API.Middlewares;
using AE.Market.Application;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Infrastructure;
using AE.Market.Infrastructure.Persistence;
using AE.Market.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace AE.Market.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = CreateWebApplication(args);
            app.Run();
        }

        public static WebApplication CreateWebApplication(
            string[] args,
            Action<WebApplicationBuilder>? configureBuilder = null)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Allow test code to override configuration before services register
            configureBuilder?.Invoke(builder);

            // Add services to the container.
            builder.Host.UseSerilog(
                (context, loggerConfig) =>
                    loggerConfig.ReadFrom.Configuration(context.Configuration)
            );

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<PermissionBasedAuthFilter>();
            })
            .AddApplicationPart(typeof(Program).Assembly);
            builder.Services.AddOpenApi();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails(options =>
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
                    ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
                    ctx.ProblemDetails.Instance =
                        $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
                }
            );

            // Add Services from other layers
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddScoped<ICurrentUser, CurrentUserService>();

            var app = builder.Build();

            // Auto-apply migrations and seed in development
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
                seeder.SeedAsync().GetAwaiter().GetResult();
            }

            // For integration tests: create from model (fresh DB each run)
            if (app.Environment.IsEnvironment("IntegrationTest"))
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
                seeder.SeedAsync().GetAwaiter().GetResult();
            }
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseExceptionHandler();
            app.UseSerilogRequestLogging();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
