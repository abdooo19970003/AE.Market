using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
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
}
