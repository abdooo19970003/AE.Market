using AE.Market.Domain.Aggregates.Analytics.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Analytics;

public sealed class ProductViewCountTests
{
    private static Product CreateTestProduct()
    {
        return Product.Create(
            Guid.NewGuid(),
            "Test Product",
            "test-product",
            "TP-001",
            Guid.NewGuid(),
            ProductType.Simple,
            "Test details");
    }

    [Fact]
    public void IncrementViewCount_IncreasesCountByOne()
    {
        var product = CreateTestProduct();

        product.IncrementViewCount();

        product.ViewCount.Should().Be(1);
    }

    [Fact]
    public void IncrementViewCount_CalledTwice_IncreasesToTwo()
    {
        var product = CreateTestProduct();

        product.IncrementViewCount();
        product.IncrementViewCount();

        product.ViewCount.Should().Be(2);
    }

    [Fact]
    public void IncrementViewCount_RaisesProductViewedDomainEvent()
    {
        var product = CreateTestProduct();
        product.ClearDomainEvents();

        product.IncrementViewCount();

        product.DomainEvents.Should().Contain(e => e is ProductViewedDomainEvent);
        var evt = product.DomainEvents.OfType<ProductViewedDomainEvent>().Single();
        evt.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public void ViewCount_DefaultsToZero()
    {
        var product = CreateTestProduct();

        product.ViewCount.Should().Be(0);
    }
}
