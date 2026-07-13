using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Catalog.Products.Variants;

public sealed class ProductVariant : BaseEntity, IMetaData
{
    public string Name { get; private set; } = string.Empty;
    public Sku Sku { get; private set; } = null!;
    public ProductStatus Status { get; private set; } = ProductStatus.Active;
    public Guid ProductId { get; private set; }

    public decimal ListPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public byte[] RowVersion { get; private set; } = [];

    private readonly List<ProductAttributeValue> _attributeValues = [];
    public IReadOnlyCollection<ProductAttributeValue> AttributeValues =>
        _attributeValues.AsReadOnly();

    private readonly List<VariantImage> _images = [];
    public IReadOnlyCollection<VariantImage> Images => _images.AsReadOnly();

    public string? MetaTitle { get; private set; }

    public string? MetaDescription { get; private set; }

    public string? MetaKeywords { get; private set; }

    private ProductVariant(Guid id, Guid productId, string name, Sku sku)
        : base(id)
    {
        ProductId = productId;
        Name = name;
        Sku = sku;
    }

    private ProductVariant() { }

    internal static ProductVariant Create(Guid id, Guid productId, string name, string sku)
    {
        return new ProductVariant(id, productId, name, Sku.Create(sku));
    }

    internal void UpdateDetails(string name, string sku)
    {
        Name = name;
        Sku = Sku.Create(sku);
        UpdateLastModified();
    }

    internal void Activate(IReadOnlyCollection<Guid>? requiredAttributeIds = null)
    {
        if (Status == ProductStatus.Active)
            return;
        if (requiredAttributeIds is not null && !HasAllRequiredAttributes(requiredAttributeIds))
            throw new DomainException(
                CatalogErrors.VariantMissingRequiredAttributes.Code,
                CatalogErrors.VariantMissingRequiredAttributes.Message
            );
        Status = ProductStatus.Active;
        UpdateLastModified();
    }

    internal IReadOnlyCollection<Guid> GetMissingRequiredAttributeIds(
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

    internal bool HasAllRequiredAttributes(IReadOnlyCollection<Guid> requiredAttributeIds)
    {
        return GetMissingRequiredAttributeIds(requiredAttributeIds).Count == 0;
    }

    internal void Deactivate()
    {
        if (Status != ProductStatus.Active)
            return;
        Status = ProductStatus.Suspended;
        UpdateLastModified();
    }

    internal ProductAttributeValue SetAttributeValue(
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
            return existing;
        }

        var value = ProductAttributeValue.Create(
            valueId,
            attributeId,
            null,
            Id,
            null,
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

    internal void RemoveAttributeValue(ProductAttributeValue value)
    {
        value.Delete();
        _attributeValues.Remove(value);
        UpdateLastModified();
    }

    internal VariantImage AddImage(
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
        var image = VariantImage.Create(imageId, Id, url, altText, isPrimary, sortOrder);
        _images.Add(image);
        UpdateLastModified();
        return image;
    }

    internal void RemoveImage(VariantImage image)
    {
        image.Delete();
        _images.Remove(image);
        UpdateLastModified();
    }

    internal void SetOrUpdateListPrice(decimal price)
    {
        ListPrice = price;
        UpdateLastModified();
    }

    internal void SetQuantity(int quantity)
    {
        if (quantity < ReservedQuantity)
            throw new DomainException(
                CatalogErrors.InsufficientStock.Code,
                CatalogErrors.InsufficientStock.Message
            );
        ApplyStockChange(quantity);
    }

    internal void AdjustStock(int delta)
    {
        var newQuantity = StockQuantity + delta;
        if (newQuantity < 0)
            throw new DomainException(
                CatalogErrors.InsufficientStock.Code,
                CatalogErrors.InsufficientStock.Message
            );
        ApplyStockChange(newQuantity);
    }

    internal void ReserveStock(int quantity)
    {
        if (quantity > AvailableQuantity)
            throw new DomainException(
                CatalogErrors.InsufficientStock.Code,
                CatalogErrors.InsufficientStock.Message
            );
        ReservedQuantity += quantity;
        UpdateLastModified();
    }

    internal void ReleaseStock(int quantity)
    {
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        UpdateLastModified();
    }

    private void ApplyStockChange(int newQuantity)
    {
        StockQuantity = newQuantity;
        UpdateLastModified();
    }

    public override void Delete()
    {
        foreach (var image in _images)
            image.Delete();
        foreach (var attrVal in _attributeValues)
            attrVal.Delete();
        Status = ProductStatus.Suspended;
        base.Delete();
    }

    public override void Restore()
    {
        Status = ProductStatus.Active;
        base.Restore();
    }

    internal void SetOrUpdateMetaFields(string? title, string? description, string? keywords)
    {
        MetaTitle = title;
        MetaDescription = description;
        MetaKeywords = keywords;
        UpdateLastModified();
    }
}
