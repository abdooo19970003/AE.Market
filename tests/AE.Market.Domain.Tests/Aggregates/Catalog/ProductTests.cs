using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class ProductTests
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

    private static Brand CreateValidBrand()
    {
        return Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");
    }

    private static ProductTaxCode CreateValidTaxCode()
    {
        return ProductTaxCode.Create(Guid.NewGuid(), "txcd_34021000", "physical", null, "Mobile Phones", "Mobile phones");
    }

    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_ReturnsProduct()
        {
            var id = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var product = Product.Create(id, "Xiaomi 12 Pro", "xiaomi-12-pro", "XI-12P-256-BLK", categoryId, ProductType.Configurable);

            product.Id.Should().Be(id);
            product.Name.Should().Be("Xiaomi 12 Pro");
            product.Slug.Value.Should().Be("xiaomi-12-pro");
            product.Sku.Value.Should().Be("XI-12P-256-BLK");
            product.CategoryId.Should().Be(categoryId);
            product.ProductType.Should().Be(ProductType.Configurable);
            product.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Create_RaisesProductCreatedDomainEvent()
        {
            var product = CreateValidProduct();

            product.DomainEvents.Should().ContainSingle(e => e is ProductCreatedDomainEvent)
                .Which.As<ProductCreatedDomainEvent>().ProductId.Should().Be(product.Id);
        }
    }

    public sealed class UpdateDetails
    {
        [Fact]
        public void UpdateDetails_UpdatesProperties()
        {
            var product = CreateValidProduct();

            product.UpdateDetails("New Name", "New details", "Meta Title", "Meta Desc", "kw1,kw2");

            product.Name.Should().Be("New Name");
            product.Details.Should().Be("New details");
            product.MetaTitle.Should().Be("Meta Title");
            product.MetaDescription.Should().Be("Meta Desc");
            product.MetaKeywords.Should().Be("kw1,kw2");
        }

        [Fact]
        public void UpdateDetails_RaisesProductDetailsUpdatedDomainEvent()
        {
            var product = CreateValidProduct();

            product.UpdateDetails("New Name", null, null, null, null);

            product.DomainEvents.Should().Contain(e => e is ProductDetailsUpdatedDomainEvent);
        }
    }

    public sealed class Variants
    {
        [Fact]
        public void AddVariant_AddsToCollection()
        {
            var product = CreateValidProduct();

            var variant = product.AddVariant(Guid.NewGuid(), "Black 256GB", "XI-12P-256-BLK");

            product.Variants.Should().ContainSingle().Which.Should().Be(variant);
            variant.ProductId.Should().Be(product.Id);
        }

        [Fact]
        public void AddVariant_RaisesProductVariantAddedDomainEvent()
        {
            var product = CreateValidProduct();

            product.AddVariant(Guid.NewGuid(), "Black 256GB", "XI-12P-256-BLK");

            product.DomainEvents.Should().Contain(e => e is ProductVariantAddedDomainEvent);
        }

        [Fact]
        public void RemoveVariant_RemovesAndSoftDeletes()
        {
            var product = CreateValidProduct();
            var variant1 = product.AddVariant(Guid.NewGuid(), "Black 256GB", "XI-12P-256-BLK");
            var variant2 = product.AddVariant(Guid.NewGuid(), "White 256GB", "XI-12P-256-WHT");

            product.RemoveVariant(variant1);

            product.Variants.Should().ContainSingle().Which.Should().Be(variant2);
            variant1.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void RemoveVariant_CascadesToVariantImages()
        {
            var product = CreateValidProduct();
            var variant1 = product.AddVariant(Guid.NewGuid(), "Black 256GB", "XI-12P-256-BLK");
            var variant2 = product.AddVariant(Guid.NewGuid(), "White 256GB", "XI-12P-256-WHT");
            var image = variant1.AddImage(Guid.NewGuid(), "img.jpg", "alt", true);

            product.RemoveVariant(variant1);

            variant1.IsDeleted.Should().BeTrue();
            image.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class Images
    {
        [Fact]
        public void AddImage_AddsToCollection()
        {
            var product = CreateValidProduct();

            var image = product.AddImage(Guid.NewGuid(), "img.jpg", "alt", true);

            product.Images.Should().ContainSingle().Which.Should().Be(image);
        }

        [Fact]
        public void AddImage_WithPrimary_ClearsOtherPrimary()
        {
            var product = CreateValidProduct();
            var img1 = product.AddImage(Guid.NewGuid(), "img1.jpg", "alt1", true);

            var img2 = product.AddImage(Guid.NewGuid(), "img2.jpg", "alt2", true);

            img1.IsPrimary.Should().BeFalse();
            img2.IsPrimary.Should().BeTrue();
        }

        [Fact]
        public void RemoveImage_RemovesAndSoftDeletes()
        {
            var product = CreateValidProduct();
            var image = product.AddImage(Guid.NewGuid(), "img.jpg", "alt", true);

            product.RemoveImage(image);

            product.Images.Should().BeEmpty();
            image.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class Tags
    {
        [Fact]
        public void AddTag_AddsToCollection()
        {
            var product = CreateValidProduct();

            product.AddTag(Guid.NewGuid(), "New Arrival", "new-arrival");

            product.Tags.Should().ContainSingle();
            product.Tags.Single().Name.Should().Be("New Arrival");
        }

        [Fact]
        public void AddTag_WithDuplicateSlug_DoesNotAdd()
        {
            var product = CreateValidProduct();

            product.AddTag(Guid.NewGuid(), "New Arrival", "new-arrival");
            product.AddTag(Guid.NewGuid(), "New Arrival 2", "new-arrival");

            product.Tags.Should().HaveCount(1);
        }

        [Fact]
        public void RemoveTag_BySlug_RemovesAndSoftDeletes()
        {
            var product = CreateValidProduct();
            product.AddTag(Guid.NewGuid(), "New Arrival", "new-arrival");
            var tag = product.Tags.Single();

            product.RemoveTag("new-arrival");

            product.Tags.Should().BeEmpty();
            tag.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void RemoveTag_WithUnknownSlug_DoesNothing()
        {
            var product = CreateValidProduct();
            product.AddTag(Guid.NewGuid(), "New Arrival", "new-arrival");

            product.RemoveTag("nonexistent");

            product.Tags.Should().HaveCount(1);
        }
    }

    public sealed class BusinessMethods
    {
        [Fact]
        public void ChangeCategory_UpdatesCategoryId()
        {
            var product = CreateValidProduct();
            var newCategoryId = Guid.NewGuid();

            product.ChangeCategory(newCategoryId);

            product.CategoryId.Should().Be(newCategoryId);
        }

        [Fact]
        public void ChangeCategory_RaisesProductCategoryChangedDomainEvent()
        {
            var product = CreateValidProduct();

            product.ChangeCategory(Guid.NewGuid());

            product.DomainEvents.Should().Contain(e => e is ProductCategoryChangedDomainEvent);
        }

        [Fact]
        public void UpdateBrand_UpdatesBrandId()
        {
            var product = CreateValidProduct();
            var brandId = Guid.NewGuid();

            product.UpdateBrand(brandId);

            product.BrandId.Should().Be(brandId);
        }

        [Fact]
        public void UpdateTaxCode_UpdatesTaxCodeId()
        {
            var product = CreateValidProduct();
            var taxCodeId = Guid.NewGuid();

            product.UpdateTaxCode(taxCodeId);

            product.TaxCodeId.Should().Be(taxCodeId);
        }

        [Fact]
        public void UpdateProductType_ChangesType()
        {
            var product = CreateValidProduct();

            product.UpdateProductType(ProductType.Simple);

            product.ProductType.Should().Be(ProductType.Simple);
        }
    }

    public sealed class ActivateDeactivate
    {
        [Fact]
        public void Deactivate_SetsIsActiveFalse()
        {
            var product = CreateValidProduct();

            product.Deactivate();

            product.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_SetsIsActiveTrue()
        {
            var product = CreateValidProduct();
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            product.Deactivate();

            product.Activate();

            product.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_RaisesProductDeactivatedDomainEvent()
        {
            var product = CreateValidProduct();

            product.Deactivate();

            product.DomainEvents.Should().Contain(e => e is ProductDeactivatedDomainEvent);
        }
    }

    public sealed class DeleteCascade
    {
        [Fact]
        public void Delete_CascadesToVariants()
        {
            var product = CreateValidProduct();
            var variant = product.AddVariant(Guid.NewGuid(), "Variant", "SKU-001");

            product.Delete();

            product.IsDeleted.Should().BeTrue();
            product.IsActive.Should().BeFalse();
            variant.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_CascadesToImages()
        {
            var product = CreateValidProduct();
            var image = product.AddImage(Guid.NewGuid(), "img.jpg", "alt", true);

            product.Delete();

            image.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_CascadesToTags()
        {
            var product = CreateValidProduct();
            product.AddTag(Guid.NewGuid(), "Tag", "tag");
            var tag = product.Tags.Single();

            product.Delete();

            tag.IsDeleted.Should().BeTrue();
        }
    }
}
