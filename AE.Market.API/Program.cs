
using AE.Market.API.Authantication;
using AE.Market.API.Exceptions;
using AE.Market.API.Middleware;
using AE.Market.API.Options;
using AE.Market.Application;
using AE.Market.Infrastructure;
using AE.Market.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
namespace AE.Market.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<PermissionBasedAuthFilter>();
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
                ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
                ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";

            });

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
            }
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseExceptionHandler();
            app.UseSerilogRequestLogging();
            
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
