using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Catalog.Products.Variants;

public sealed class ProductVariant : BaseEntity, IMetaData
{
    public string Name { get; private set; }
    public Sku Sku { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid ProductId { get; private set; }

    public decimal SalePrice { get; private set; }
    public decimal ListPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;

    private readonly List<VariantAttributeValue> _attributeValues = [];
    public IReadOnlyCollection<VariantAttributeValue> AttributeValues => _attributeValues.AsReadOnly();

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

    internal void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdateLastModified();
    }

    internal void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdateLastModified();
    }

    internal VariantAttributeValue SetAttributeValue(Guid valueId, Guid attributeId, AttributeInputType inputType,
        string? textValue = null,
        int? integerValue = null, decimal? decimalValue = null, bool? booleanValue = null,
        DateTime? dateTimeValue = null, Guid? optionId = null)
    {
        var existing = _attributeValues.Find(av => av.AttributeId == attributeId);
        if (existing is not null)
        {
            existing.UpdateValue(inputType, textValue, integerValue, decimalValue, booleanValue, dateTimeValue, optionId);
            return existing;
        }

        var value = VariantAttributeValue.Create(valueId, Id, attributeId, inputType, textValue, integerValue,
            decimalValue, booleanValue, dateTimeValue, optionId);
        _attributeValues.Add(value);
        UpdateLastModified();
        return value;
    }

    internal void RemoveAttributeValue(VariantAttributeValue value)
    {
        value.Delete();
        _attributeValues.Remove(value);
        UpdateLastModified();
    }

    internal VariantImage AddImage(Guid imageId, string url, string? altText, bool isPrimary = false, int sortOrder = 0)
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

    internal void SetOrUpdateSellingPrice(decimal price)
    {
        var oldPrice = SalePrice;
        SalePrice = price;
        AddDomainEvent(new VariantPriceChangedDomainEvent(ProductId, Id, oldPrice, price));
        UpdateLastModified();
    }

    internal void SetOrUpdateListPrice(decimal price)
    {
        ListPrice = price;
        UpdateLastModified();
    }

    internal void SetQuantity(int quantity)
    {
        var oldQuantity = StockQuantity;
        StockQuantity = quantity;
        AddDomainEvent(new VariantStockAdjustedDomainEvent(ProductId, Id, oldQuantity, quantity, quantity - oldQuantity));
        UpdateLastModified();
    }

    internal void AdjustStock(int delta)
    {
        var oldQuantity = StockQuantity;
        StockQuantity += delta;
        AddDomainEvent(new VariantStockAdjustedDomainEvent(ProductId, Id, oldQuantity, StockQuantity, delta));
        UpdateLastModified();
    }

    internal void ReserveStock(int quantity)
    {
        if (quantity > AvailableQuantity)
            throw new DomainException(CatalogErrors.InsufficientStock.Code, CatalogErrors.InsufficientStock.Message);
        ReservedQuantity += quantity;
        UpdateLastModified();
    }

    internal void ReleaseStock(int quantity)
    {
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        UpdateLastModified();
    }

    public override void Delete()
    {
        foreach (var image in _images)
            image.Delete();
        foreach (var attrVal in _attributeValues)
            attrVal.Delete();
        IsActive = false;
        base.Delete();
    }

    public override void Restore()
    {
        IsActive = true;
        base.Restore();
    }
    public void SetOrUpdateMetaFields(string? title, string? description, string? keywords)
    {
        MetaTitle = title;
        MetaDescription = description;
        MetaKeywords = keywords;
        UpdateLastModified();
    }
}
