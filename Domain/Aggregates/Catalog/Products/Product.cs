using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Catalog.Products;

public sealed class Product : BaseEntity, IAggregateRoot, IMetaData
{
    public string Name { get; private set; }
    public Slug Slug { get; private set; }
    public Sku Sku { get; private set; }
    public string Details { get; private set; }
    public string? ShortDescription { get; private set; }
    public string? LongDescription { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Active;
    public URL Url => URL.Create("products", Slug);
    public bool IsPurchasable =>
        Status == ProductStatus.Active
        && (
            ProductType is ProductType.Simple or ProductType.Digital
            || (ProductType == ProductType.Bundle && _bundleItems.Count > 0)
            || _variants.Any(v => v.Status == ProductStatus.Active)
        );

    public Guid BrandId { get; private set; }

    public ProductType ProductType { get; private set; }

    public bool AllowBackOrder { get; private set; }
    public int? BackOrderLimit { get; private set; }

    // SEO
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }

    // Weak reference to Category aggregate root
    public Guid CategoryId { get; private set; }

    public Guid TaxCodeId { get; private set; }

    private readonly List<ProductVariant> _variants = [];
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    private readonly List<ProductImage> _images = [];
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<Tag> _tags = [];
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    private readonly List<ProductAttributeValue> _attributeValues = [];
    public IReadOnlyCollection<ProductAttributeValue> AttributeValues =>
        _attributeValues.AsReadOnly();

    private readonly List<ProductRelation> _relations = [];
    public IReadOnlyCollection<ProductRelation> Relations => _relations.AsReadOnly();

    public decimal SalePrice =>
        _variants.Where(v => v.Status == ProductStatus.Active && v.SalePrice > 0).Min(v => (decimal?)v.SalePrice) ?? 0m;
    public int StockQuantity => _variants.Sum(v => v.StockQuantity);

    // FOR Bundles
    private readonly List<BundleItem> _bundleItems = [];
    public IReadOnlyCollection<BundleItem> BundleItems => _bundleItems.AsReadOnly();

    private Product(
        Guid id,
        string name,
        Slug slug,
        Sku sku,
        Guid categoryId,
        ProductType type,
        string? details
    )
        : base(id)
    {
        Name = name;
        Slug = slug;
        Sku = sku;
        CategoryId = categoryId;
        Details = details ?? string.Empty;
        ProductType = type;
    }

    private Product() { }

    public static Product Create(
        Guid id,
        string name,
        string slug,
        string sku,
        Guid categoryId,
        ProductType type,
        string? details = null
    )
    {
        var product = new Product(
            id,
            name,
            Slug.Create(slug),
            Sku.Create(sku),
            categoryId,
            type,
            details
        );
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id));
        return product;
    }

    public void UpdateDetails(
        string name,
        string? details,
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords
    )
    {
        Name = name;
        Details = details ?? string.Empty;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        AddDomainEvent(new ProductDetailsUpdatedDomainEvent(Id, name, Details));
        if (metaTitle is not null || metaDescription is not null || metaKeywords is not null)
            AddDomainEvent(
                new ProductMetaFieldsUpdatedDomainEvent(
                    Id,
                    metaTitle,
                    metaDescription,
                    metaKeywords
                )
            );
        UpdateLastModified();
    }

    public void UpdateProductType(ProductType type)
    {
        if (type == ProductType.Configurable
            && !_attributeValues.Any(av => !av.IsDeleted && av.IsVariantDefiner))
            throw new DomainException(
                CatalogErrors.ProductTypeRequiresSuperAttributes.Code,
                CatalogErrors.ProductTypeRequiresSuperAttributes.Message
            );
        var oldType = ProductType;
        ProductType = type;
        AddDomainEvent(new ProductTypeChangedDomainEvent(Id, oldType, type));
        UpdateLastModified();
    }

    public void UpdateSlug(string slug)
    {
        var oldSlug = Slug;
        Slug = Slug.Create(slug);
        AddDomainEvent(new ProductSlugChangedDomainEvent(Id, oldSlug.Value, slug));
        UpdateLastModified();
    }

    public void ChangeCategory(Guid categoryId)
    {
        var oldCategoryId = CategoryId;
        CategoryId = categoryId;
        AddDomainEvent(new ProductCategoryChangedDomainEvent(Id, oldCategoryId, categoryId));
        UpdateLastModified();
    }

    public void UpdateBrand(Guid brandId)
    {
        var oldBrandId = BrandId;
        BrandId = brandId;
        AddDomainEvent(new ProductBrandChangedDomainEvent(Id, oldBrandId, brandId));
        UpdateLastModified();
    }

    public void UpdateTaxCode(Guid taxCodeId)
    {
        var oldTaxCodeId = TaxCodeId;
        TaxCodeId = taxCodeId;
        AddDomainEvent(new ProductTaxCodeChangedDomainEvent(Id, oldTaxCodeId, taxCodeId));
        UpdateLastModified();
    }

    public void Activate(IReadOnlyCollection<Guid>? requiredAttributeIds = null)
    {
        if (Status == ProductStatus.Active)
            return;
        if (ProductType is not (ProductType.Simple or ProductType.Digital or ProductType.Bundle) && _variants.Count == 0)
            throw new DomainException(
                CatalogErrors.ProductNoVariants.Code,
                CatalogErrors.ProductNoVariants.Message
            );
        if (ProductType == ProductType.Bundle && _bundleItems.Count == 0)
            throw new DomainException(
                CatalogErrors.BundleProductHasNoItems.Code,
                CatalogErrors.BundleProductHasNoItems.Message
            );
        if (ProductType == ProductType.Configurable
            && !_attributeValues.Any(av => !av.IsDeleted && av.IsVariantDefiner))
            throw new DomainException(
                CatalogErrors.ProductMissingSuperAttributes.Code,
                CatalogErrors.ProductMissingSuperAttributes.Message
            );
        if (requiredAttributeIds is not null && !HasAllRequiredAttributes(requiredAttributeIds))
            throw new DomainException(
                CatalogErrors.ProductMissingRequiredAttributes.Code,
                CatalogErrors.ProductMissingRequiredAttributes.Message
            );
        Status = ProductStatus.Active;
        AddDomainEvent(new ProductActivatedDomainEvent(Id));
        UpdateLastModified();
    }

    public void Deactivate()
    {
        if (Status != ProductStatus.Active)
            return;
        Status = ProductStatus.Suspended;
        AddDomainEvent(new ProductDeactivatedDomainEvent(Id));
        UpdateLastModified();
    }

    public void ActivateVariant(Guid variantId, IReadOnlyCollection<Guid>? requiredAttributeIds = null)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new DomainException(
                CatalogErrors.VariantNotFound.Code,
                CatalogErrors.VariantNotFound.Message
            );

        variant.Activate(requiredAttributeIds);
        AddDomainEvent(new VariantActivatedDomainEvent(Id, variantId));
        UpdateLastModified();
    }

    public void DeactivateVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new DomainException(
                CatalogErrors.VariantNotFound.Code,
                CatalogErrors.VariantNotFound.Message
            );

        variant.Deactivate();
        AddDomainEvent(new VariantDeactivatedDomainEvent(Id, variantId));
        UpdateLastModified();
    }

    public ProductVariant AddVariant(Guid variantId, string name, string sku)
    {
        var variant = ProductVariant.Create(variantId, Id, name, sku);
        _variants.Add(variant);
        AddDomainEvent(new ProductVariantAddedDomainEvent(Id, variantId, name, sku));
        UpdateLastModified();
        return variant;
    }

    public void RemoveVariant(ProductVariant variant)
    {
        if (_variants.Count <= 1 && Status == ProductStatus.Active)
            throw new DomainException(
                CatalogErrors.CannotRemoveLastVariant.Code,
                CatalogErrors.CannotRemoveLastVariant.Message
            );
        variant.Delete();
        _variants.Remove(variant);
        AddDomainEvent(new ProductVariantRemovedDomainEvent(Id, variant.Id));
        UpdateLastModified();
    }

    public ProductImage AddImage(
        Guid imageId,
        string url,
        string? altText,
        bool isPrimary = false,
        int sortOrder = 0
    )
    {
        if (isPrimary)
        {
            foreach (var img in _images)
                img.ClearPrimary();
        }
        var image = ProductImage.Create(imageId, Id, url, altText, isPrimary, sortOrder);
        _images.Add(image);
        AddDomainEvent(new ProductImageAddedDomainEvent(Id, imageId, url, isPrimary));
        UpdateLastModified();
        return image;
    }

    public void RemoveImage(ProductImage image)
    {
        image.Delete();
        _images.Remove(image);
        AddDomainEvent(new ProductImageRemovedDomainEvent(Id, image.Id));
        UpdateLastModified();
    }

    public void AddTag(Guid tagId, string name, string slug)
    {
        if (_tags.Any(t => t.Slug.Value == slug))
            return;
        var tag = Tag.Create(tagId, name, slug);
        _tags.Add(tag);
        AddDomainEvent(new ProductTagAddedDomainEvent(Id, tagId, name, slug));
        UpdateLastModified();
    }

    public void RemoveTag(string slug)
    {
        var tag = _tags.FirstOrDefault(t => t.Slug.Value == slug);
        if (tag is not null)
        {
            tag.Delete();
            _tags.Remove(tag);
            AddDomainEvent(new ProductTagRemovedDomainEvent(Id, slug));
            UpdateLastModified();
        }
    }

    public void SetOrUpdateMetaFields(string? title, string? description, string? keywords)
    {
        MetaTitle = title;
        MetaDescription = description;
        MetaKeywords = keywords;
        AddDomainEvent(new ProductMetaFieldsUpdatedDomainEvent(Id, title, description, keywords));
        UpdateLastModified();
    }

    public void SetShortDescription(string? shortDescription)
    {
        ShortDescription = shortDescription;
        AddDomainEvent(new ProductShortDescriptionChangedDomainEvent(Id, shortDescription));
        UpdateLastModified();
    }

    public void SetLongDescription(string? longDescription)
    {
        LongDescription = longDescription;
        AddDomainEvent(new ProductLongDescriptionChangedDomainEvent(Id, longDescription));
        UpdateLastModified();
    }

    public void SetAllowBackOrder(bool allowBackOrder, int? backOrderLimit = null)
    {
        AllowBackOrder = allowBackOrder;
        BackOrderLimit = allowBackOrder ? backOrderLimit : null;
        UpdateLastModified();
    }

    public ProductRelation AddRelation(
        Guid relationId,
        Guid relatedProductId,
        RelationType type,
        int sortOrder = 0
    )
    {
        if (relatedProductId == Id)
            throw new DomainException(
                CatalogErrors.ProductCannotRelateToSelf.Code,
                CatalogErrors.ProductCannotRelateToSelf.Message
            );

        if (_relations.Any(r => r.RelatedProductId == relatedProductId && r.Type == type))
            return _relations.First(r => r.RelatedProductId == relatedProductId && r.Type == type);

        var relation = ProductRelation.Create(relationId, Id, relatedProductId, type, sortOrder);
        _relations.Add(relation);
        AddDomainEvent(new ProductRelationAddedDomainEvent(Id, relatedProductId, type));
        UpdateLastModified();
        return relation;
    }

    public void RemoveRelation(ProductRelation relation)
    {
        relation.Delete();
        _relations.Remove(relation);
        AddDomainEvent(
            new ProductRelationRemovedDomainEvent(Id, relation.RelatedProductId, relation.Type)
        );
        UpdateLastModified();
    }

    public void SetVariantQuantity(Guid variantId, int quantity)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            throw new DomainException(
                CatalogErrors.VariantNotFound.Code,
                CatalogErrors.VariantNotFound.Message
            );

        var oldQuantity = variant.StockQuantity;
        variant.SetQuantity(quantity);
        AddDomainEvent(
            new VariantStockAdjustedDomainEvent(
                Id,
                variantId,
                oldQuantity,
                quantity,
                quantity - oldQuantity
            )
        );
        UpdateLastModified();
    }

    public void AdjustVariantStock(Guid variantId, int delta)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            throw new DomainException(
                CatalogErrors.VariantNotFound.Code,
                CatalogErrors.VariantNotFound.Message
            );

        var oldQuantity = variant.StockQuantity;
        variant.AdjustStock(delta);
        var newQuantity = variant.StockQuantity;
        AddDomainEvent(
            new VariantStockAdjustedDomainEvent(Id, variantId, oldQuantity, newQuantity, delta)
        );
        UpdateLastModified();
    }

    public void ReserveVariantStock(Guid variantId, int quantity)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            throw new DomainException(
                CatalogErrors.VariantNotFound.Code,
                CatalogErrors.VariantNotFound.Message
            );

        variant.ReserveStock(quantity);
        UpdateLastModified();
    }

    public void ReleaseVariantStock(Guid variantId, int quantity)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            throw new DomainException(
                CatalogErrors.VariantNotFound.Code,
                CatalogErrors.VariantNotFound.Message
            );

        variant.ReleaseStock(quantity);
        UpdateLastModified();
    }

    public void SetVariantSalePrice(Guid variantId, decimal price)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            throw new DomainException(
                CatalogErrors.VariantNotFound.Code,
                CatalogErrors.VariantNotFound.Message
            );

        var oldPrice = variant.SalePrice;
        variant.SetOrUpdateSellingPrice(price);
        AddDomainEvent(new VariantPriceChangedDomainEvent(Id, variantId, oldPrice, price));
        UpdateLastModified();
    }

    public ProductAttributeValue SetVariantAttributeValue(
        Guid variantId,
        Guid valueId,
        Guid attributeId,
        AttributeInputType inputType,
        string? textValue = null,
        int? integerValue = null,
        decimal? decimalValue = null,
        bool? booleanValue = null,
        DateTime? dateTimeValue = null,
        Guid? optionId = null
    )
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new DomainException(
                CatalogErrors.VariantNotFound.Code,
                CatalogErrors.VariantNotFound.Message
            );

        var result = variant.SetAttributeValue(
            valueId,
            attributeId,
            inputType,
            textValue,
            integerValue,
            decimalValue,
            booleanValue,
            dateTimeValue,
            optionId
        );

        UpdateLastModified();
        return result;
    }

    public ProductAttributeValue SetAttributeValue(
        Guid valueId,
        Guid attributeId,
        AttributeInputType inputType,
        bool? isVariantDefiner,
        string? textValue = null,
        int? integerValue = null,
        decimal? decimalValue = null,
        bool? booleanValue = null,
        DateTime? dateTimeValue = null,
        Guid? optionId = null
    )
    {
        var existing = _attributeValues.Find(av => av.AttributeId == attributeId);
        if (existing is not null)
        {
            existing.UpdateValue(
                inputType,
                textValue,
                integerValue,
                decimalValue,
                booleanValue,
                dateTimeValue,
                optionId
            );

            if (isVariantDefiner.HasValue && isVariantDefiner.Value != existing.IsVariantDefiner)
            {
                if (isVariantDefiner.Value)
                {
                    if (ProductType != ProductType.Configurable)
                        throw new DomainException(
                            CatalogErrors.SuperAttributeNotAllowedForProductType.Code,
                            CatalogErrors.SuperAttributeNotAllowedForProductType.Message
                        );
                }
                else
                {
                    if (ProductType == ProductType.Configurable
                        && _attributeValues.Count(av => !av.IsDeleted && av.IsVariantDefiner && av.AttributeId != attributeId) == 0)
                        throw new DomainException(
                            CatalogErrors.CannotRemoveLastSuperAttribute.Code,
                            CatalogErrors.CannotRemoveLastSuperAttribute.Message
                        );
                }
                existing.UpdateIsVariantDefiner(isVariantDefiner.Value);
            }

            UpdateLastModified();
            return existing;
        }

        if (isVariantDefiner == true && ProductType != ProductType.Configurable)
            throw new DomainException(
                CatalogErrors.SuperAttributeNotAllowedForProductType.Code,
                CatalogErrors.SuperAttributeNotAllowedForProductType.Message
            );

        var value = ProductAttributeValue.Create(
            valueId,
            attributeId,
            Id,
            null,
            isVariantDefiner,
            inputType,
            textValue,
            integerValue,
            decimalValue,
            booleanValue,
            dateTimeValue,
            optionId
        );
        _attributeValues.Add(value);
        UpdateLastModified();
        return value;
    }

    public void RemoveAttributeValue(ProductAttributeValue value)
    {
        if (value.IsVariantDefiner
            && ProductType == ProductType.Configurable
            && _attributeValues.Count(av => !av.IsDeleted && av.IsVariantDefiner && av.AttributeId != value.AttributeId) == 0)
            throw new DomainException(
                CatalogErrors.CannotRemoveLastSuperAttribute.Code,
                CatalogErrors.CannotRemoveLastSuperAttribute.Message
            );
        value.Delete();
        _attributeValues.Remove(value);
        UpdateLastModified();
    }

    public IReadOnlyCollection<Guid> GetMissingRequiredAttributeIds(
        IReadOnlyCollection<Guid> requiredAttributeIds
    )
    {
        var coveredAttributes = _attributeValues
            .Where(v => !v.IsDeleted)
            .Select(v => v.AttributeId)
            .ToHashSet();

        return requiredAttributeIds
            .Where(id => !coveredAttributes.Contains(id))
            .ToList()
            .AsReadOnly();
    }

    public bool HasAllRequiredAttributes(IReadOnlyCollection<Guid> requiredAttributeIds)
    {
        return GetMissingRequiredAttributeIds(requiredAttributeIds).Count == 0;
    }

    public override void Delete()
    {
        foreach (var relation in _relations)
            relation.Delete();
        foreach (var variant in _variants)
            variant.Delete();
        foreach (var image in _images)
            image.Delete();
        foreach (var tag in _tags)
            tag.Delete();
        foreach (var attrVal in _attributeValues)
            attrVal.Delete();
        foreach (var bundleItem in _bundleItems)
            bundleItem.Delete();
        Status = ProductStatus.Suspended;
        AddDomainEvent(new ProductDeletedDomainEvent(Id));
        base.Delete();
    }

    public override void Restore()
    {
        if (ProductType is not (ProductType.Simple or ProductType.Digital or ProductType.Bundle) && _variants.Count == 0)
            throw new DomainException(
                CatalogErrors.ProductNoVariants.Code,
                CatalogErrors.ProductNoVariants.Message
            );
        if (ProductType == ProductType.Bundle && _bundleItems.Count == 0)
            throw new DomainException(
                CatalogErrors.BundleProductHasNoItems.Code,
                CatalogErrors.BundleProductHasNoItems.Message
            );
        if (ProductType == ProductType.Configurable
            && !_attributeValues.Any(av => !av.IsDeleted && av.IsVariantDefiner))
            throw new DomainException(
                CatalogErrors.ProductMissingSuperAttributes.Code,
                CatalogErrors.ProductMissingSuperAttributes.Message
            );
        Status = ProductStatus.Active;
        base.Restore();
    }

    public BundleItem AddBundleItem(Guid id, Guid itemId, int quantity)
    {
        if (ProductType != ProductType.Bundle)
            throw new DomainException(
                CatalogErrors.CannotAddBundleItemToNonBundleProduct.Code,
                CatalogErrors.CannotAddBundleItemToNonBundleProduct.Message
            );
        var item = new BundleItem(id, Id, itemId, quantity);
        _bundleItems.Add(item);
        AddDomainEvent(new BundleItemAddedDomainEvent(Id, id, itemId, quantity));
        UpdateLastModified();
        return item;
    }

    public void RemoveBundleItem(Guid bundleItemId)
    {
        var item =
            _bundleItems.FirstOrDefault(i => i.Id == bundleItemId)
            ?? throw new DomainException(
                CatalogErrors.BundleItemNotFound.Code,
                CatalogErrors.BundleItemNotFound.Message
            );
        item.Delete();
        _bundleItems.Remove(item);
        AddDomainEvent(new BundleItemRemovedDomainEvent(Id, bundleItemId));
        UpdateLastModified();
    }
}
