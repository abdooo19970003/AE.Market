using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Exceptions;
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

        [Fact]
        public void Delete_CascadesToRelations()
        {
            var product = CreateValidProduct();
            var relation = product.AddRelation(Guid.NewGuid(), Guid.NewGuid(), RelationType.Related);

            product.Delete();

            relation.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class ActivateGuards
    {
        [Fact]
        public void Activate_ConfigurableWithNoVariants_Throws()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Test", "test", "SKU-001", Guid.NewGuid(), ProductType.Configurable);
            product.Deactivate();

            var act = () => product.Activate();

            act.Should().Throw<DomainException>();
            product.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_ConfigurableWithVariants_Succeeds()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Test", "test", "SKU-001", Guid.NewGuid(), ProductType.Configurable);
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            product.Deactivate();

            product.Activate();

            product.IsActive.Should().BeTrue();
        }
    }

    public sealed class RestoreTests
    {
        [Fact]
        public void Restore_SetsIsActiveTrueAndIsDeletedFalse()
        {
            var product = CreateValidProduct();
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            product.Delete();

            product.Restore();

            product.IsActive.Should().BeTrue();
            product.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Activate_AlreadyActive_DoesNotRaiseDuplicateEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            product.Activate();

            product.DomainEvents.Should().BeEmpty();
        }
    }

    public sealed class RelationTests
    {
        [Fact]
        public void AddRelation_SelfRelation_Throws()
        {
            var product = CreateValidProduct();

            var act = () => product.AddRelation(Guid.NewGuid(), product.Id, RelationType.Related);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void AddRelation_DuplicateRelation_ReturnsExisting()
        {
            var product = CreateValidProduct();
            var relatedId = Guid.NewGuid();
            var relation = product.AddRelation(Guid.NewGuid(), relatedId, RelationType.Related);

            var result = product.AddRelation(Guid.NewGuid(), relatedId, RelationType.Related);

            result.Should().Be(relation);
            product.Relations.Should().ContainSingle();
        }

        [Fact]
        public void AddRelation_DifferentRelationType_CreatesNew()
        {
            var product = CreateValidProduct();
            var relatedId = Guid.NewGuid();
            product.AddRelation(Guid.NewGuid(), relatedId, RelationType.Related);

            var result = product.AddRelation(Guid.NewGuid(), relatedId, RelationType.CrossSell);

            product.Relations.Should().HaveCount(2);
            result.Type.Should().Be(RelationType.CrossSell);
        }

        [Fact]
        public void RemoveRelation_SoftDeletesAndRemovesFromCollection()
        {
            var product = CreateValidProduct();
            var relation = product.AddRelation(Guid.NewGuid(), Guid.NewGuid(), RelationType.Related);

            product.RemoveRelation(relation);

            product.Relations.Should().BeEmpty();
            relation.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_CascadesToRelations()
        {
            var product = CreateValidProduct();
            var relation = product.AddRelation(Guid.NewGuid(), Guid.NewGuid(), RelationType.Related);

            product.Delete();

            relation.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class VariantProxyMethods
    {
        [Fact]
        public void SetVariantQuantity_UnknownVariantId_Throws()
        {
            var product = CreateValidProduct();

            var act = () => product.SetVariantQuantity(Guid.NewGuid(), 10);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void AdjustVariantStock_UnknownVariantId_Throws()
        {
            var product = CreateValidProduct();

            var act = () => product.AdjustVariantStock(Guid.NewGuid(), 10);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void ReserveVariantStock_UnknownVariantId_Throws()
        {
            var product = CreateValidProduct();

            var act = () => product.ReserveVariantStock(Guid.NewGuid(), 10);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void ReleaseVariantStock_UnknownVariantId_Throws()
        {
            var product = CreateValidProduct();

            var act = () => product.ReleaseVariantStock(Guid.NewGuid(), 10);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void SetVariantSalePrice_UnknownVariantId_Throws()
        {
            var product = CreateValidProduct();

            var act = () => product.SetVariantSalePrice(Guid.NewGuid(), 10m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void ReserveVariantStock_DoesNotRaiseDomainEvent()
        {
            var product = CreateValidProduct();
            var variant = product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            product.SetVariantQuantity(variant.Id, 10);
            product.ClearDomainEvents();

            product.ReserveVariantStock(variant.Id, 3);

            product.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void ReleaseVariantStock_DoesNotRaiseDomainEvent()
        {
            var product = CreateValidProduct();
            var variant = product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            product.SetVariantQuantity(variant.Id, 10);
            product.ReserveVariantStock(variant.Id, 5);
            product.ClearDomainEvents();

            product.ReleaseVariantStock(variant.Id, 3);

            product.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void SetVariantSalePrice_RaisesVariantPriceChangedDomainEvent()
        {
            var product = CreateValidProduct();
            var variant = product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            product.ClearDomainEvents();

            product.SetVariantSalePrice(variant.Id, 99.99m);

            product.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<VariantPriceChangedDomainEvent>()
                .Which.Should().BeEquivalentTo(new
                {
                    ProductId = product.Id,
                    VariantId = variant.Id,
                    OldPrice = 0m,
                    NewPrice = 99.99m
                });
        }

        [Fact]
        public void SetVariantQuantity_EventCarriesCorrectData()
        {
            var product = CreateValidProduct();
            var variant = product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            product.SetVariantQuantity(variant.Id, 10);
            product.ClearDomainEvents();

            product.SetVariantQuantity(variant.Id, 25);

            var evt = product.DomainEvents.OfType<VariantStockAdjustedDomainEvent>().Single();
            evt.ProductId.Should().Be(product.Id);
            evt.VariantId.Should().Be(variant.Id);
            evt.OldQuantity.Should().Be(10);
            evt.NewQuantity.Should().Be(25);
            evt.Delta.Should().Be(15);
        }

        [Fact]
        public void AdjustVariantStock_EventCarriesCorrectData()
        {
            var product = CreateValidProduct();
            var variant = product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            product.SetVariantQuantity(variant.Id, 10);
            product.ClearDomainEvents();

            product.AdjustVariantStock(variant.Id, -3);

            var evt = product.DomainEvents.OfType<VariantStockAdjustedDomainEvent>().Single();
            evt.ProductId.Should().Be(product.Id);
            evt.VariantId.Should().Be(variant.Id);
            evt.OldQuantity.Should().Be(10);
            evt.NewQuantity.Should().Be(7);
            evt.Delta.Should().Be(-3);
        }

        [Fact]
        public void SetVariantQuantity_EventRaisedOnProductNotVariant()
        {
            var product = CreateValidProduct();
            var variant = product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            product.ClearDomainEvents();

            product.SetVariantQuantity(variant.Id, 50);

            product.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<VariantStockAdjustedDomainEvent>();
            variant.DomainEvents.Should().BeEmpty();
        }
    }

    public sealed class BundleItems
    {
        [Fact]
        public void AddBundleItem_ToBundleProduct_AddsToCollection()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);
            var itemId = Guid.NewGuid();

            var bundleItem = product.AddBundleItem(Guid.NewGuid(), itemId, 2);

            product.BundleItems.Should().ContainSingle();
            bundleItem.BundleId.Should().Be(product.Id);
            bundleItem.ItemId.Should().Be(itemId);
            bundleItem.Quantity.Should().Be(2);
        }

        [Fact]
        public void AddBundleItem_RaisesBundleItemAddedDomainEvent()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);
            var itemId = Guid.NewGuid();
            var bundleItemId = Guid.NewGuid();

            product.AddBundleItem(bundleItemId, itemId, 3);

            product.DomainEvents.Should().Contain(e => e is BundleItemAddedDomainEvent);
            var evt = product.DomainEvents.OfType<BundleItemAddedDomainEvent>().Single();
            evt.BundleId.Should().Be(product.Id);
            evt.BundleItemId.Should().Be(bundleItemId);
            evt.ItemId.Should().Be(itemId);
            evt.Quantity.Should().Be(3);
        }

        [Fact]
        public void AddBundleItem_ToNonBundleProduct_Throws()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Simple", "simple", "SMPL-001", Guid.NewGuid(), ProductType.Simple);

            var act = () => product.AddBundleItem(Guid.NewGuid(), Guid.NewGuid(), 1);

            act.Should().Throw<DomainException>();
            product.BundleItems.Should().BeEmpty();
        }

        [Fact]
        public void RemoveBundleItem_RemovesAndSoftDeletes()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);
            var bundleItemId = Guid.NewGuid();
            var item = product.AddBundleItem(bundleItemId, Guid.NewGuid(), 1);

            product.RemoveBundleItem(bundleItemId);

            product.BundleItems.Should().BeEmpty();
            item.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void RemoveBundleItem_WithUnknownId_Throws()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);

            var act = () => product.RemoveBundleItem(Guid.NewGuid());

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Delete_CascadesToBundleItems()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);
            var item = product.AddBundleItem(Guid.NewGuid(), Guid.NewGuid(), 2);

            product.Delete();

            product.IsDeleted.Should().BeTrue();
            item.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Activate_BundleWithNoItems_Throws()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);
            product.Deactivate();

            var act = () => product.Activate();

            act.Should().Throw<DomainException>();
            product.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_BundleWithItems_Succeeds()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);
            product.AddBundleItem(Guid.NewGuid(), Guid.NewGuid(), 1);
            product.Deactivate();

            product.Activate();

            product.IsActive.Should().BeTrue();
        }

        [Fact]
        public void IsPurchasable_BundleWithItems_ReturnsTrue()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);
            product.AddBundleItem(Guid.NewGuid(), Guid.NewGuid(), 1);

            product.IsPurchasable.Should().BeTrue();
        }

        [Fact]
        public void IsPurchasable_BundleWithoutItems_ReturnsFalse()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);

            product.IsPurchasable.Should().BeFalse();
        }

        [Fact]
        public void SetQuantity_UpdatesQuantity()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Bundle", "bundle", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);

            var item = product.AddBundleItem(Guid.NewGuid(), Guid.NewGuid(), 2);

            item.SetQuantity(5);

            item.Quantity.Should().Be(5);
        }
    }

    public sealed class SlugUpdate
    {
        [Fact]
        public void UpdateSlug_UpdatesSlug()
        {
            var product = CreateValidProduct();
            product.UpdateSlug("new-slug");

            product.Slug.Value.Should().Be("new-slug");
        }

        [Fact]
        public void UpdateSlug_RaisesProductSlugChangedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            product.UpdateSlug("new-slug");

            product.DomainEvents.Should().Contain(e => e is ProductSlugChangedDomainEvent);
        }
    }

    public sealed class ShortDescription
    {
        [Fact]
        public void SetShortDescription_SetsValue()
        {
            var product = CreateValidProduct();

            product.SetShortDescription("short");

            product.ShortDescription.Should().Be("short");
        }

        [Fact]
        public void SetShortDescription_WithNull_ClearsValue()
        {
            var product = CreateValidProduct();
            product.SetShortDescription("short");

            product.SetShortDescription(null);

            product.ShortDescription.Should().BeNull();
        }

        [Fact]
        public void SetShortDescription_RaisesProductShortDescriptionChangedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            product.SetShortDescription("short");

            product.DomainEvents.Should().Contain(e => e is ProductShortDescriptionChangedDomainEvent);
        }
    }

    public sealed class LongDescription
    {
        [Fact]
        public void SetLongDescription_SetsValue()
        {
            var product = CreateValidProduct();

            product.SetLongDescription("long desc");

            product.LongDescription.Should().Be("long desc");
        }

        [Fact]
        public void SetLongDescription_WithNull_ClearsValue()
        {
            var product = CreateValidProduct();
            product.SetLongDescription("long desc");

            product.SetLongDescription(null);

            product.LongDescription.Should().BeNull();
        }

        [Fact]
        public void SetLongDescription_RaisesProductLongDescriptionChangedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            product.SetLongDescription("long desc");

            product.DomainEvents.Should().Contain(e => e is ProductLongDescriptionChangedDomainEvent);
        }
    }

    public sealed class MetaFields
    {
        [Fact]
        public void SetOrUpdateMetaFields_SetsValues()
        {
            var product = CreateValidProduct();

            product.SetOrUpdateMetaFields("title", "desc", "kw1,kw2");

            product.MetaTitle.Should().Be("title");
            product.MetaDescription.Should().Be("desc");
            product.MetaKeywords.Should().Be("kw1,kw2");
        }

        [Fact]
        public void SetOrUpdateMetaFields_NullValues_ClearsFields()
        {
            var product = CreateValidProduct();
            product.SetOrUpdateMetaFields("title", "desc", "kw1,kw2");

            product.SetOrUpdateMetaFields(null, null, null);

            product.MetaTitle.Should().BeNull();
            product.MetaDescription.Should().BeNull();
            product.MetaKeywords.Should().BeNull();
        }

        [Fact]
        public void SetOrUpdateMetaFields_RaisesProductMetaFieldsUpdatedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            product.SetOrUpdateMetaFields("title", "desc", "kw1,kw2");

            product.DomainEvents.Should().Contain(e => e is ProductMetaFieldsUpdatedDomainEvent);
        }
    }

    public sealed class AllowBackOrder
    {
        [Fact]
        public void SetAllowBackOrder_WithLimit_SetsValues()
        {
            var product = CreateValidProduct();

            product.SetAllowBackOrder(true, 5);

            product.AllowBackOrder.Should().BeTrue();
            product.BackOrderLimit.Should().Be(5);
        }

        [Fact]
        public void SetAllowBackOrder_False_ClearsLimit()
        {
            var product = CreateValidProduct();
            product.SetAllowBackOrder(true, 5);

            product.SetAllowBackOrder(false);

            product.AllowBackOrder.Should().BeFalse();
            product.BackOrderLimit.Should().BeNull();
        }

        [Fact]
        public void SetAllowBackOrder_WithNullLimit_DefaultsToNull()
        {
            var product = CreateValidProduct();

            product.SetAllowBackOrder(true);

            product.AllowBackOrder.Should().BeTrue();
            product.BackOrderLimit.Should().BeNull();
        }
    }

    public sealed class IsPurchasable
    {
        [Fact]
        public void IsPurchasable_SimpleActive_ReturnsTrue()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Test", "test", "SKU-001", Guid.NewGuid(), ProductType.Simple);

            product.IsPurchasable.Should().BeTrue();
        }

        [Fact]
        public void IsPurchasable_DigitalActive_ReturnsTrue()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Test", "test", "SKU-001", Guid.NewGuid(), ProductType.Digital);

            product.IsPurchasable.Should().BeTrue();
        }

        [Fact]
        public void IsPurchasable_Inactive_ReturnsFalse()
        {
            var product = CreateValidProduct();
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");

            product.Deactivate();

            product.IsPurchasable.Should().BeFalse();
        }

        [Fact]
        public void IsPurchasable_ConfigurableNoVariants_ReturnsFalse()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Test", "test", "SKU-001", Guid.NewGuid(), ProductType.Configurable);

            product.IsPurchasable.Should().BeFalse();
        }

        [Fact]
        public void IsPurchasable_ConfigurableWithActiveVariant_ReturnsTrue()
        {
            var product = CreateValidProduct();
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");

            product.IsPurchasable.Should().BeTrue();
        }

        [Fact]
        public void IsPurchasable_BundleNoItems_ReturnsFalse()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Test", "test", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);

            product.IsPurchasable.Should().BeFalse();
        }

        [Fact]
        public void IsPurchasable_BundleWithItems_ReturnsTrue()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Test", "test", "BNDL-001", Guid.NewGuid(), ProductType.Bundle);
            product.AddBundleItem(Guid.NewGuid(), Guid.NewGuid(), 1);

            product.IsPurchasable.Should().BeTrue();
        }
    }

    public sealed class SalePrice
    {
        [Fact]
        public void SalePrice_NoVariants_ReturnsZero()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Test", "test", "SKU-001", Guid.NewGuid(), ProductType.Configurable);

            product.SalePrice.Should().Be(0m);
        }

        [Fact]
        public void SalePrice_ReturnsMinOfActiveVariantPrices()
        {
            var product = CreateValidProduct();
            var v1 = product.AddVariant(Guid.NewGuid(), "V1", "SKU-V1");
            var v2 = product.AddVariant(Guid.NewGuid(), "V2", "SKU-V2");
            product.ClearDomainEvents();
            product.SetVariantSalePrice(v1.Id, 10m);
            product.SetVariantSalePrice(v2.Id, 20m);

            product.SalePrice.Should().Be(10m);
        }

        [Fact]
        public void SalePrice_IgnoresInactiveVariants()
        {
            var product = CreateValidProduct();
            var v1 = product.AddVariant(Guid.NewGuid(), "V1", "SKU-V1");
            var v2 = product.AddVariant(Guid.NewGuid(), "V2", "SKU-V2");
            var v3 = product.AddVariant(Guid.NewGuid(), "V3", "SKU-V3");
            product.ClearDomainEvents();
            product.SetVariantSalePrice(v1.Id, 10m);
            product.SetVariantSalePrice(v2.Id, 20m);
            product.SetVariantSalePrice(v3.Id, 30m);
            v3.Deactivate();

            product.SalePrice.Should().Be(10m);
        }
    }

    public sealed class StockQuantity
    {
        [Fact]
        public void StockQuantity_NoVariants_ReturnsZero()
        {
            var product = Product.Create(
                Guid.NewGuid(), "Test", "test", "SKU-001", Guid.NewGuid(), ProductType.Configurable);

            product.StockQuantity.Should().Be(0);
        }

        [Fact]
        public void StockQuantity_SumOfVariantStock()
        {
            var product = CreateValidProduct();
            var v1 = product.AddVariant(Guid.NewGuid(), "V1", "SKU-V1");
            var v2 = product.AddVariant(Guid.NewGuid(), "V2", "SKU-V2");
            product.ClearDomainEvents();
            product.SetVariantQuantity(v1.Id, 5);
            product.ClearDomainEvents();
            product.SetVariantQuantity(v2.Id, 3);

            product.StockQuantity.Should().Be(8);
        }
    }

    public sealed class AttributeValue
    {
        [Fact]
        public void RemoveAttributeValue_RemovesAndSoftDeletes()
        {
            var product = CreateValidProduct();
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");

            var setMethod = typeof(Product).GetMethod("SetAttributeValue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            _ = setMethod.Invoke(product, [Guid.NewGuid(), Guid.NewGuid(), AttributeInputType.Text, null, "Red", null, null, null, null, null]);

            var attributeValue = product.AttributeValues.Should().ContainSingle().Subject;

            var removeMethod = typeof(Product).GetMethod("RemoveAttributeValue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            removeMethod.Invoke(product, [attributeValue]);

            product.AttributeValues.Should().BeEmpty();
            attributeValue.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class EventRaises
    {
        [Fact]
        public void AddTag_RaisesProductTagAddedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            product.AddTag(Guid.NewGuid(), "Tag", "tag");

            product.DomainEvents.Should().Contain(e => e is ProductTagAddedDomainEvent);
        }

        [Fact]
        public void RemoveTag_RaisesProductTagRemovedDomainEvent()
        {
            var product = CreateValidProduct();
            product.AddTag(Guid.NewGuid(), "Tag", "tag");
            product.ClearDomainEvents();

            product.RemoveTag("tag");

            product.DomainEvents.Should().Contain(e => e is ProductTagRemovedDomainEvent);
        }

        [Fact]
        public void AddRelation_RaisesProductRelationAddedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            _ = product.AddRelation(Guid.NewGuid(), Guid.NewGuid(), RelationType.Related);

            product.DomainEvents.Should().Contain(e => e is ProductRelationAddedDomainEvent);
        }

        [Fact]
        public void RemoveRelation_RaisesProductRelationRemovedDomainEvent()
        {
            var product = CreateValidProduct();
            var relation = product.AddRelation(Guid.NewGuid(), Guid.NewGuid(), RelationType.Related);
            product.ClearDomainEvents();

            product.RemoveRelation(relation);

            product.DomainEvents.Should().Contain(e => e is ProductRelationRemovedDomainEvent);
        }

        [Fact]
        public void AddImage_RaisesProductImageAddedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            _ = product.AddImage(Guid.NewGuid(), "img.jpg", "alt", true);

            product.DomainEvents.Should().Contain(e => e is ProductImageAddedDomainEvent);
        }

        [Fact]
        public void RemoveImage_RaisesProductImageRemovedDomainEvent()
        {
            var product = CreateValidProduct();
            var image = product.AddImage(Guid.NewGuid(), "img.jpg", "alt", true);
            product.ClearDomainEvents();

            product.RemoveImage(image);

            product.DomainEvents.Should().Contain(e => e is ProductImageRemovedDomainEvent);
        }

        [Fact]
        public void UpdateProductType_RaisesProductTypeChangedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            product.UpdateProductType(ProductType.Simple);

            product.DomainEvents.Should().Contain(e => e is ProductTypeChangedDomainEvent);
        }

        [Fact]
        public void UpdateBrand_RaisesProductBrandChangedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            product.UpdateBrand(Guid.NewGuid());

            product.DomainEvents.Should().Contain(e => e is ProductBrandChangedDomainEvent);
        }

        [Fact]
        public void UpdateTaxCode_RaisesProductTaxCodeChangedDomainEvent()
        {
            var product = CreateValidProduct();
            product.ClearDomainEvents();

            product.UpdateTaxCode(Guid.NewGuid());

            product.DomainEvents.Should().Contain(e => e is ProductTaxCodeChangedDomainEvent);
        }
    }

    public sealed class AttributeQueries
    {
        [Fact]
        public void GetMissingRequiredAttributeIds_WithAllPresent_ReturnsEmpty()
        {
            var product = CreateValidProduct();
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            var attrId1 = Guid.NewGuid();
            var attrId2 = Guid.NewGuid();
            var requiredIds = new List<Guid> { attrId1, attrId2 };

            typeof(Product).GetMethod("SetAttributeValue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(product, [Guid.NewGuid(), attrId1, AttributeInputType.Text, null, "Red", null, null, null, null, null]);
            typeof(Product).GetMethod("SetAttributeValue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(product, [Guid.NewGuid(), attrId2, AttributeInputType.Integer, null, null, 42, null, null, null, null]);

            var result = product.GetMissingRequiredAttributeIds(requiredIds);

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetMissingRequiredAttributeIds_WithMissing_ReturnsMissing()
        {
            var product = CreateValidProduct();
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            var presentId = Guid.NewGuid();
            var missingId = Guid.NewGuid();
            var requiredIds = new List<Guid> { presentId, missingId };

            typeof(Product).GetMethod("SetAttributeValue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(product, [Guid.NewGuid(), presentId, AttributeInputType.Text, null, "Red", null, null, null, null, null]);

            var result = product.GetMissingRequiredAttributeIds(requiredIds);

            result.Should().ContainSingle();
            result.First().Should().Be(missingId);
        }

        [Fact]
        public void HasAllRequiredAttributes_WhenAllPresent_ReturnsTrue()
        {
            var product = CreateValidProduct();
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");
            var attrId = Guid.NewGuid();

            typeof(Product).GetMethod("SetAttributeValue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(product, [Guid.NewGuid(), attrId, AttributeInputType.Text, null, "Red", null, null, null, null, null]);

            product.HasAllRequiredAttributes(new List<Guid> { attrId }).Should().BeTrue();
        }

        [Fact]
        public void HasAllRequiredAttributes_WhenMissing_ReturnsFalse()
        {
            var product = CreateValidProduct();
            product.AddVariant(Guid.NewGuid(), "Default", "SKU-DEF-001");

            product.HasAllRequiredAttributes(new List<Guid> { Guid.NewGuid() }).Should().BeFalse();
        }
    }
}
