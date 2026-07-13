using AE.Market.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace AE.Market.Integration.Tests;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    private WebApplication? _app;

    public HttpClient HttpClient { get; private set; } = null!;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("marketDb")
        .WithUsername("postgres")
        .WithPassword("password")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        var jwtSecret = "FXRSBWkbM6maMusUKEgFuF634TYgXxSb";

        Environment.SetEnvironmentVariable("ConnectionStrings__Database", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__redis", $"{_redis.GetConnectionString()},abortConnect=false");
        Environment.SetEnvironmentVariable("Jwt__Secret", jwtSecret);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "AE.Market.API");
        Environment.SetEnvironmentVariable("Jwt__Audience", "developers");
        Environment.SetEnvironmentVariable("Jwt__ExpirationInMinutes", "60");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");

        _app = Program.CreateWebApplication([], builder =>
        {
            builder.WebHost.UseTestServer();
        });

        await _app.StartAsync();
        HttpClient = _app.GetTestClient();
    }

    public IServiceScope CreateScope()
    {
        return _app!.Services.CreateScope();
    }

    public async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__redis", null);
        Environment.SetEnvironmentVariable("Jwt__Secret", null);
        Environment.SetEnvironmentVariable("Jwt__Issuer", null);
        Environment.SetEnvironmentVariable("Jwt__Audience", null);
        Environment.SetEnvironmentVariable("Jwt__ExpirationInMinutes", null);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

        if (_app is not null)
            await _app.DisposeAsync();

        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}
