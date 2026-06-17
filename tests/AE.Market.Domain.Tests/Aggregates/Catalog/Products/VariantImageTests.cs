using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.Products;

public sealed class VariantImageTests
{
    private static ProductVariant CreateVariant(out Product product)
    {
        product = Product.Create(
            Guid.NewGuid(),
            "Test Product",
            "test-product",
            "SKU-001",
            Guid.NewGuid(),
            ProductType.Configurable
        );
        return product.AddVariant(Guid.NewGuid(), "Black 256GB", "XI-12P-256-BLK");
    }

    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_SetsAllProperties()
        {
            var variant = CreateVariant(out _);
            var id = Guid.NewGuid();

            var image = VariantImage.Create(id, variant.Id, "img.jpg", "alt text", true, 5);

            image.Id.Should().Be(id);
            image.VariantId.Should().Be(variant.Id);
            image.Url.Should().Be("img.jpg");
            image.AltText.Should().Be("alt text");
            image.IsPrimary.Should().BeTrue();
            image.SortOrder.Should().Be(5);
        }
    }

    public sealed class PrimaryToggle
    {
        [Fact]
        public void SetPrimary_SetsIsPrimaryTrue()
        {
            var variant = CreateVariant(out _);
            var image = VariantImage.Create(Guid.NewGuid(), variant.Id, "img.jpg", null);

            image.SetPrimary();

            image.IsPrimary.Should().BeTrue();
        }

        [Fact]
        public void ClearPrimary_SetsIsPrimaryFalse()
        {
            var variant = CreateVariant(out _);
            var image = VariantImage.Create(Guid.NewGuid(), variant.Id, "img.jpg", null, isPrimary: true);

            image.ClearPrimary();

            image.IsPrimary.Should().BeFalse();
        }
    }

    public sealed class Update
    {
        [Fact]
        public void Update_UpdatesUrlAltTextSortOrder()
        {
            var variant = CreateVariant(out _);
            var image = VariantImage.Create(Guid.NewGuid(), variant.Id, "img.jpg", "old alt", false, 1);

            image.Update("new.jpg", "new alt", 10);

            image.Url.Should().Be("new.jpg");
            image.AltText.Should().Be("new alt");
            image.SortOrder.Should().Be(10);
        }
    }
}
