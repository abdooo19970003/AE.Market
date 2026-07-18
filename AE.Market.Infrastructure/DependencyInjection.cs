using System.Text;
using AE.Market.Infrastructure.Authentication.Options;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Infrastructure.Authentication;
using AE.Market.Infrastructure.Caching;
using AE.Market.Infrastructure.Persistence;
using AE.Market.Infrastructure.Persistence.Interceptors;
using AE.Market.Infrastructure.Persistence.Outbox;
using AE.Market.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
using AE.Market.Infrastructure.Persistence.Seeders;
using AE.Market.Infrastructure.Search;
using Elastic.Clients.Elasticsearch;

namespace AE.Market.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services
                .AddDatabases(configuration)
                .AddOutbox()
                .AddCache(configuration)
                .AddAuth(configuration)
                .AddSearch(configuration);

            return services;
        }

        private static IServiceCollection AddDatabases(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            string connectionString =
                configuration.GetConnectionString("Database")
                ?? throw new ArgumentNullException("Default ConnectionString");

            // DbContexts
            services.AddDbContext<AppDbContext>(
                (sp, options) =>
                {
                    options.UseNpgsql(connectionString);
                    options.AddInterceptors(sp.GetService<DomainEventDispatcher>()!);
                }
            );

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));

            // Seeder
            services.AddScoped<DbSeeder>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services)
        {
            // Interceptors
            services.AddSingleton<DomainEventDispatcher>();

            // Quartz
            services.AddQuartz(cfg =>
            {
                var jobKey = new JobKey(nameof(OutboxProcessorJob));
                cfg.AddJob<OutboxProcessorJob>(jobKey)
                    .AddTrigger(trigger =>
                        trigger
                            .ForJob(jobKey)
                            .WithSimpleSchedule(schedule =>
                                schedule.WithIntervalInSeconds(100).RepeatForever()
                            )
                    );

                var syncJobKey = new JobKey(nameof(SyncProductsJob));
                cfg.AddJob<SyncProductsJob>(syncJobKey)
                    .AddTrigger(trigger =>
                        trigger
                            .ForJob(syncJobKey)
                            .WithCronSchedule("0 0 2 * * ?")
                    );
            });
            services.AddQuartzHostedService();
            return services;
        }

        private static IServiceCollection AddSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var uri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
            var defaultIndex = configuration["Elasticsearch:DefaultIndex"] ?? "products";

            var settings = new ElasticsearchClientSettings(new Uri(uri))
                .DefaultIndex(defaultIndex)
                .EnableDebugMode()
                .PrettyJson();

            var client = new ElasticsearchClient(settings);
            services.AddSingleton(client);
            services.AddScoped<IElasticsearchService, ElasticsearchService>();

            return services;
        }

        private static IServiceCollection AddCache(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // redis connection
            string redisConnectionString =
                configuration.GetConnectionString("redis")
                ?? throw new ArgumentNullException("Redis ConnectionString");

            var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);

            // fusion setup

            services
                .AddFusionCache()
                .WithDefaultEntryOptions(options =>
                {
                    options.Duration = TimeSpan.FromMinutes(30);

                    options.IsFailSafeEnabled = true;
                    options.FailSafeMaxDuration = TimeSpan.FromHours(2);
                    options.FailSafeThrottleDuration = TimeSpan.FromSeconds(30);

                    options.EagerRefreshThreshold = 0.9f;
                    options.FactorySoftTimeout = TimeSpan.FromMilliseconds(100);
                    options.FactoryHardTimeout = TimeSpan.FromMilliseconds(1500);

                    options.DistributedCacheSoftTimeout = TimeSpan.FromMilliseconds(1000);
                    options.DistributedCacheHardTimeout = TimeSpan.FromSeconds(2);
                    options.AllowBackgroundDistributedCacheOperations = true;
                })
                .WithSerializer(new FusionCacheSystemTextJsonSerializer())
                .WithDistributedCache(
                    new RedisCache(
                        new RedisCacheOptions() { Configuration = redisConnectionString }
                    )
                )
                .WithBackplane(
                    new RedisBackplane(
                        new RedisBackplaneOptions() { Configuration = redisConnectionString }
                    )
                );

            services.AddMemoryCache();
            //services.AddSingleton<ICacheService, MemoryCachingService>();
            services.AddSingleton<ICacheService, FusionCachingService>();
            return services;
        }

        private static IServiceCollection AddAuth(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            JwtOptions? jwtOptions = configuration.GetSection(JwtOptions.Section).Get<JwtOptions>();

            if (jwtOptions is null)
                throw new ArgumentNullException(nameof(jwtOptions));

            services
                .AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateTokenReplay = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.Secret!)
                        ),
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        ClockSkew = TimeSpan.Zero,
                        ValidateLifetime = true,
                    };
                });
            services.AddHttpContextAccessor();
            services.AddSingleton<JwtOptions>(jwtOptions);
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddSingleton<IJwtService, JwtService>();

            return services;
        }
    }
}
