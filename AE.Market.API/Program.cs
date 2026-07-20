using AE.Market.API.Authentication;
using AE.Market.API.Configuration;
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
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Security.Claims;
using System.Threading.RateLimiting;
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

            // Security: CORS
            var corsOptions = new CorsOptions();
            builder.Configuration.GetSection("Cors").Bind(corsOptions);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AdminPolicy", policy =>
                    policy.WithOrigins(corsOptions.AdminOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader());

                options.AddPolicy("PublicPolicy", policy =>
                    policy.WithOrigins(corsOptions.PublicOrigins)
                          .WithMethods("GET")
                          .AllowAnyHeader());

                options.AddPolicy("DefaultPolicy", policy =>
                    policy.WithOrigins(corsOptions.DefaultOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });

            // Security: Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddPolicy("auth", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddPolicy("admin", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User?.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
                            ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 60,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.OnRejected = async (context, ct) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                    }
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "Too many requests",
                        retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra)
                            ? (int)ra.TotalSeconds : 60
                    }, ct);
                };
            });

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

            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseCors("DefaultPolicy");

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<JwtExpiryMiddleware>();
            app.UseRateLimiter();
            app.MapControllers();

            // Ensure ES indices exist on startup
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var esService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();
                esService.EnsureIndicesAsync().GetAwaiter().GetResult();
            }

            app.MapHealthChecks("/health");
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            });

            return app;
        }
    }
}
