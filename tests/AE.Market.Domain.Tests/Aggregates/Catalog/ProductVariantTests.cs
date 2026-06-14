using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class ProductVariantTests
{
    private static Product CreateProductWithVariant(out ProductVariant variant)
    {
        var product = Product.Create(
            Guid.NewGuid(),
            "Test Product",
            "test-product",
            "SKU-001",
            Guid.NewGuid(),
            ProductType.Configurable
        );
        variant = product.AddVariant(Guid.NewGuid(), "Black 256GB", "XI-12P-256-BLK");
        return product;
    }

    public sealed class UpdateDetails
    {
        [Fact]
        public void UpdateDetails_UpdatesProperties()
        {
            var product = CreateProductWithVariant(out var variant);

            variant.UpdateDetails("White 128GB", "XI-12P-128-WHT");

            variant.Name.Should().Be("White 128GB");
            variant.Sku.Value.Should().Be("XI-12P-128-WHT");
        }
    }

    public sealed class AttributeValues
    {
        [Fact]
        public void SetAttributeValue_AddsNewValue()
        {
            var product = CreateProductWithVariant(out var variant);
            var attrId = Guid.NewGuid();

            var result = variant.SetAttributeValue(Guid.NewGuid(), attrId, AttributeInputType.Text, "Red");

            variant.AttributeValues.Should().ContainSingle()
                .Which.AttributeId.Should().Be(attrId);
        }

        [Fact]
        public void SetAttributeValue_UpdatesExistingValue()
        {
            var product = CreateProductWithVariant(out var variant);
            var attrId = Guid.NewGuid();
            var valueId = Guid.NewGuid();

            variant.SetAttributeValue(valueId, attrId, AttributeInputType.Text, "Red");
            variant.SetAttributeValue(valueId, attrId, AttributeInputType.Text, "Blue");

            variant.AttributeValues.Should().ContainSingle();
            variant.AttributeValues.Single().ValueText.Should().Be("Blue");
        }

        [Fact]
        public void RemoveAttributeValue_RemovesAndSoftDeletes()
        {
            var product = CreateProductWithVariant(out var variant);
            var attrId = Guid.NewGuid();
            variant.SetAttributeValue(Guid.NewGuid(), attrId, AttributeInputType.Text, "Red");
            var attrValue = variant.AttributeValues.Single();

            variant.RemoveAttributeValue(attrValue);

            variant.AttributeValues.Should().BeEmpty();
            attrValue.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class Images
    {
        [Fact]
        public void AddImage_AddsToCollection()
        {
            var product = CreateProductWithVariant(out var variant);

            variant.AddImage(Guid.NewGuid(), "img.jpg", "alt", true);

            variant.Images.Should().ContainSingle();
        }

        [Fact]
        public void RemoveImage_RemovesAndSoftDeletes()
        {
            var product = CreateProductWithVariant(out var variant);
            variant.AddImage(Guid.NewGuid(), "img.jpg", "alt", true);
            var image = variant.Images.Single();

            variant.RemoveImage(image);

            variant.Images.Should().BeEmpty();
            image.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class DeleteCascade
    {
        [Fact]
        public void Delete_CascadesToImages()
        {
            var product = CreateProductWithVariant(out var variant);
            variant.AddImage(Guid.NewGuid(), "img.jpg", "alt", true);
            var image = variant.Images.Single();

            variant.Delete();

            variant.IsDeleted.Should().BeTrue();
            variant.IsActive.Should().BeFalse();
            image.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_CascadesToAttributeValues()
        {
            var product = CreateProductWithVariant(out var variant);
            variant.SetAttributeValue(Guid.NewGuid(), Guid.NewGuid(), AttributeInputType.Text, "Red");
            var attrVal = variant.AttributeValues.Single();

            variant.Delete();

            attrVal.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class SetQuantity
    {
        [Fact]
        public void SetQuantity_UpdatesStockAndFiresEvent()
        {
            var product = CreateProductWithVariant(out var variant);
            product.ClearDomainEvents();

            product.SetVariantQuantity(variant.Id, 50);

            variant.StockQuantity.Should().Be(50);
            variant.AvailableQuantity.Should().Be(50);
            product.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<VariantStockAdjustedDomainEvent>();
        }

        [Fact]
        public void SetQuantity_BelowReservedQuantity_Throws()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);
            product.ReserveVariantStock(variant.Id, 5);
            product.ClearDomainEvents();

            var act = () => product.SetVariantQuantity(variant.Id, 3);

            act.Should().Throw<DomainException>();
            variant.StockQuantity.Should().Be(10);
        }

        [Fact]
        public void SetQuantity_ExactlyReservedQuantity_Succeeds()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);
            product.ReserveVariantStock(variant.Id, 5);

            product.SetVariantQuantity(variant.Id, 5);

            variant.StockQuantity.Should().Be(5);
            variant.AvailableQuantity.Should().Be(0);
        }
    }

    public sealed class AdjustStock
    {
        [Fact]
        public void AdjustStock_PositiveDelta_IncreasesStock()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);

            product.AdjustVariantStock(variant.Id, 5);

            variant.StockQuantity.Should().Be(15);
            variant.AvailableQuantity.Should().Be(15);
        }

        [Fact]
        public void AdjustStock_NegativeDelta_DecreasesStock()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);

            product.AdjustVariantStock(variant.Id, -3);

            variant.StockQuantity.Should().Be(7);
            variant.AvailableQuantity.Should().Be(7);
        }

        [Fact]
        public void AdjustStock_NegativeResult_Throws()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 5);

            var act = () => product.AdjustVariantStock(variant.Id, -10);

            act.Should().Throw<DomainException>();
            variant.StockQuantity.Should().Be(5);
        }

        [Fact]
        public void AdjustStock_FiresEvent()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);
            product.ClearDomainEvents();

            product.AdjustVariantStock(variant.Id, -2);

            var evt = product.DomainEvents.OfType<VariantStockAdjustedDomainEvent>().Single();
            evt.Delta.Should().Be(-2);
            evt.OldQuantity.Should().Be(10);
            evt.NewQuantity.Should().Be(8);
        }
    }

    public sealed class ReserveStock
    {
        [Fact]
        public void ReserveStock_IncreasesReservedQuantity()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);

            product.ReserveVariantStock(variant.Id, 3);

            variant.ReservedQuantity.Should().Be(3);
            variant.AvailableQuantity.Should().Be(7);
        }

        [Fact]
        public void ReserveStock_ExceedsAvailable_Throws()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);

            var act = () => product.ReserveVariantStock(variant.Id, 15);

            act.Should().Throw<DomainException>();
            variant.ReservedQuantity.Should().Be(0);
        }

        [Fact]
        public void ReserveStock_ExactlyAvailable_Succeeds()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);

            product.ReserveVariantStock(variant.Id, 10);

            variant.ReservedQuantity.Should().Be(10);
            variant.AvailableQuantity.Should().Be(0);
        }

        [Fact]
        public void ReserveStock_MultipleReservations_Accumulate()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);

            product.ReserveVariantStock(variant.Id, 3);
            product.ReserveVariantStock(variant.Id, 4);

            variant.ReservedQuantity.Should().Be(7);
            variant.AvailableQuantity.Should().Be(3);
        }

        [Fact]
        public void ReserveStock_AfterSecondReservationExceeds_Throws()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);
            product.ReserveVariantStock(variant.Id, 8);

            var act = () => product.ReserveVariantStock(variant.Id, 5);

            act.Should().Throw<DomainException>();
            variant.ReservedQuantity.Should().Be(8);
        }
    }

    public sealed class ReleaseStock
    {
        [Fact]
        public void ReleaseStock_DecreasesReservedQuantity()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);
            product.ReserveVariantStock(variant.Id, 5);

            product.ReleaseVariantStock(variant.Id, 3);

            variant.ReservedQuantity.Should().Be(2);
            variant.AvailableQuantity.Should().Be(8);
        }

        [Fact]
        public void ReleaseStock_MoreThanReserved_ClampsToZero()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);
            product.ReserveVariantStock(variant.Id, 3);

            product.ReleaseVariantStock(variant.Id, 10);

            variant.ReservedQuantity.Should().Be(0);
            variant.AvailableQuantity.Should().Be(10);
        }

        [Fact]
        public void ReleaseStock_AllReserved_AvailableRestored()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);
            product.ReserveVariantStock(variant.Id, 10);

            product.ReleaseVariantStock(variant.Id, 10);

            variant.ReservedQuantity.Should().Be(0);
            variant.AvailableQuantity.Should().Be(10);
        }
    }

    public sealed class EventEnforcement
    {
        [Fact]
        public void SetQuantity_ViaProduct_DoesNotRaiseEventOnVariant()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 50);
            product.ClearDomainEvents();

            product.SetVariantQuantity(variant.Id, 100);

            variant.DomainEvents.Should().BeEmpty();
            product.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<VariantStockAdjustedDomainEvent>();
        }

        [Fact]
        public void AdjustStock_ViaProduct_DoesNotRaiseEventOnVariant()
        {
            var product = CreateProductWithVariant(out var variant);
            product.SetVariantQuantity(variant.Id, 10);
            product.ClearDomainEvents();

            product.AdjustVariantStock(variant.Id, -3);

            variant.DomainEvents.Should().BeEmpty();
            product.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<VariantStockAdjustedDomainEvent>();
        }

        [Fact]
        public void SetOrUpdateSellingPrice_ViaProduct_DoesNotRaiseEventOnVariant()
        {
            var product = CreateProductWithVariant(out var variant);
            product.ClearDomainEvents();

            product.SetVariantSalePrice(variant.Id, 299.99m);

            variant.DomainEvents.Should().BeEmpty();
            product.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<VariantPriceChangedDomainEvent>();
        }
    }
}
