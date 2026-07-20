using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Search.DTOs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AE.Market.Integration.Tests.Search;

[Collection("Integration tests")]
public sealed class SearchIntegrationTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task Search_products_returns_results_after_indexing()
    {
        using var scope = factory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

        await service.EnsureIndicesAsync();

        var productId = Guid.NewGuid();
        var doc = new ProductDocument
        {
            Id = productId,
            Name = "Wireless Mouse",
            Slug = "wireless-mouse",
            Sku = "WM-001",
            ShortDescription = "A wireless mouse",
            Status = "Active",
            ProductType = "Simple",
            CategoryId = Guid.NewGuid(),
            CategoryName = "Electronics",
            BrandId = Guid.NewGuid(),
            BrandName = "TestBrand",
            ListPrice = 29.99m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await service.IndexProductAsync(doc);

        var query = new SearchProductsQuery { Q = "Wireless" };
        var result = await service.SearchProductsAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();
        result.Items.Should().Contain(i => i.Name.Contains("Wireless"));
    }

    [Fact]
    public async Task Search_products_with_no_results_returns_empty()
    {
        using var scope = factory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

        await service.EnsureIndicesAsync();

        var query = new SearchProductsQuery { Q = "zzz_nonexistent_product_xyz" };
        var result = await service.SearchProductsAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteProduct_removes_from_search_results()
    {
        using var scope = factory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

        await service.EnsureIndicesAsync();

        var productId = Guid.NewGuid();
        var doc = new ProductDocument
        {
            Id = productId,
            Name = "DeleteMe Product",
            Slug = "deleteme-product",
            Sku = "DEL-001",
            Status = "Active",
            ProductType = "Simple",
            CategoryId = Guid.NewGuid(),
            CategoryName = "Test",
            BrandId = Guid.NewGuid(),
            BrandName = "Test",
            ListPrice = 10m,
            StockQuantity = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await service.IndexProductAsync(doc);

        var query = new SearchProductsQuery { Q = "DeleteMe" };
        var before = await service.SearchProductsAsync(query);
        before.Items.Should().NotBeEmpty();

        await service.DeleteProductAsync(productId);

        var after = await service.SearchProductsAsync(query);
        after.Items.Should().BeEmpty();
    }
}
