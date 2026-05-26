using AE.Market.Application.Common.Interfaces;
using AE.Market.Infrastructure.Authantication;
using AE.Market.Infrastructure.Caching;
using AE.Market.Infrastructure.Persistence;
using AE.Market.Infrastructure.Persistence.Interceptors;
using AE.Market.Infrastructure.Persistence.Outbox;
using AE.Market.Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace AE.Market.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddDatabases(configuration).AddOutbox().AddCache(configuration).AddAuth();

            return services;
        }

        public static IServiceCollection AddDatabases(
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
            services.AddScoped<IUnitOfWork,UnitOfWork>();

            // Repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }

        public static IServiceCollection AddOutbox(this IServiceCollection services)
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
                                schedule.WithIntervalInSeconds(5).RepeatForever()
                            )
                    );
            });
            services.AddQuartzHostedService();
            return services;
        }

        public static IServiceCollection AddCache(
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

        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services.AddSingleton<IPasswordService, PasswordService>();

            return services;
        }
    }
}
