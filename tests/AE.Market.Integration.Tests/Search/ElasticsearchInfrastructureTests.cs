using AE.Market.Application.Common.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AE.Market.Integration.Tests.Search;

[Collection("Integration tests")]
public sealed class ElasticsearchInfrastructureTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task ElasticsearchService_is_registered()
    {
        using var scope = factory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task EnsureIndicesAsync_creates_indices()
    {
        using var scope = factory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

        var result = await service.EnsureIndicesAsync();

        result.Should().NotBeNull();
    }
}
