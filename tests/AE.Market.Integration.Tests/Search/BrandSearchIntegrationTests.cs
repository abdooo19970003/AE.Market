using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Search.DTOs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AE.Market.Integration.Tests.Search;

[Collection("Integration tests")]
public sealed class BrandSearchIntegrationTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task IndexBrandAsync_indexes_brand_without_error()
    {
        using var scope = factory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

        await service.EnsureIndicesAsync();

        var doc = new BrandDocument
        {
            Id = Guid.NewGuid(),
            Name = "Apple",
            Slug = "apple",
            IsActive = true,
            ProductCount = 5
        };

        var act = () => service.IndexBrandAsync(doc);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateBrandAsync_updates_existing_brand()
    {
        using var scope = factory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

        await service.EnsureIndicesAsync();

        var brandId = Guid.NewGuid();
        var doc = new BrandDocument
        {
            Id = brandId,
            Name = "Samsung",
            Slug = "samsung",
            IsActive = true,
            ProductCount = 3
        };

        await service.IndexBrandAsync(doc);

        var updated = doc with { ProductCount = 10 };

        var act = () => service.UpdateBrandAsync(brandId, updated);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteBrandAsync_removes_brand_without_error()
    {
        using var scope = factory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

        await service.EnsureIndicesAsync();

        var brandId = Guid.NewGuid();
        var doc = new BrandDocument
        {
            Id = brandId,
            Name = "Nike",
            Slug = "nike",
            IsActive = true,
            ProductCount = 0
        };

        await service.IndexBrandAsync(doc);

        var act = () => service.DeleteBrandAsync(brandId);
        await act.Should().NotThrowAsync();
    }
}
