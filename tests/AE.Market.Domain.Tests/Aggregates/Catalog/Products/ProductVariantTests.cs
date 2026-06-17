using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.Products;

public sealed class ProductVariantTests
{
    private static Product CreateValidProduct()
    {
        return Product.Create(
            Guid.NewGuid(),
            "Xiaomi 12 Pro",
            "xiaomi-12-pro",
            "XI-12P-256-BLK",
            Guid.NewGuid(),
            ProductType.Configurable
        );
    }

    private static ProductVariant CreateVariant()
    {
        var product = CreateValidProduct();
        return product.AddVariant(Guid.NewGuid(), "Black 256GB", "XI-12P-256-BLK");
    }

    public sealed class ActivateDeactivate
    {
        [Fact]
        public void Activate_SetsIsActiveTrue()
        {
            var variant = CreateVariant();
            variant.Deactivate();

            variant.Activate();

            variant.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_SetsIsActiveFalse()
        {
            var variant = CreateVariant();

            variant.Deactivate();

            variant.IsActive.Should().BeFalse();
        }
    }

    public sealed class MetaFields
    {
        [Fact]
        public void SetOrUpdateMetaFields_SetsValues()
        {
            var variant = CreateVariant();

            variant.SetOrUpdateMetaFields("Meta Title", "Meta Desc", "kw1,kw2");

            variant.MetaTitle.Should().Be("Meta Title");
            variant.MetaDescription.Should().Be("Meta Desc");
            variant.MetaKeywords.Should().Be("kw1,kw2");
        }
    }

    public sealed class DeleteRestore
    {
        [Fact]
        public void Delete_SoftDeletes()
        {
            var variant = CreateVariant();

            variant.Delete();

            variant.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Restore_AfterDelete_ClearsIsDeleted()
        {
            var variant = CreateVariant();
            variant.Delete();

            variant.Restore();

            variant.IsDeleted.Should().BeFalse();
        }
    }

    public sealed class ListPrice
    {
        [Fact]
        public void SetOrUpdateListPrice_UpdatesPrice()
        {
            var variant = CreateVariant();

            variant.SetOrUpdateListPrice(99.99m);

            variant.ListPrice.Should().Be(99.99m);
        }
    }
}
