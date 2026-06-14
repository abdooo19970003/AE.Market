using AE.Market.Domain.Aggregates.Catalog.Products;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.Products;

public sealed class ProductImageTests
{
    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var image = ProductImage.Create(id, productId, "https://example.com/img.jpg", "Alt text",
            isPrimary: true, sortOrder: 3);

        image.Id.Should().Be(id);
        image.ProductId.Should().Be(productId);
        image.Url.Should().Be("https://example.com/img.jpg");
        image.AltText.Should().Be("Alt text");
        image.IsPrimary.Should().BeTrue();
        image.SortOrder.Should().Be(3);
    }

    [Fact]
    public void SetPrimary_SetsIsPrimaryTrue()
    {
        var image = ProductImage.Create(Guid.NewGuid(), Guid.NewGuid(), "https://example.com/img.jpg", null);

        image.SetPrimary();

        image.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void ClearPrimary_SetsIsPrimaryFalse()
    {
        var image = ProductImage.Create(Guid.NewGuid(), Guid.NewGuid(), "https://example.com/img.jpg", null,
            isPrimary: true);

        image.ClearPrimary();

        image.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void Update_UpdatesUrlAltTextSortOrder()
    {
        var image = ProductImage.Create(Guid.NewGuid(), Guid.NewGuid(), "https://example.com/old.jpg", "Old");

        image.Update("https://example.com/new.jpg", "New", sortOrder: 5);

        image.Url.Should().Be("https://example.com/new.jpg");
        image.AltText.Should().Be("New");
        image.SortOrder.Should().Be(5);
    }
}
