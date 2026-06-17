using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Products;

public sealed class ProductAttributeValue : BaseEntity
{
    public Guid AttributeId { get; private set; }

    // If assigned at the Product level (e.g., Weight = 1.2kg, Brand = Nike)
    public Guid? ProductId { get; private set; }

    // If assigned at the Variant level (e.g., Color = Red, Size = XL)
    public Guid? VariantId { get; private set; }

    // If TRUE, this attribute is a "Super-Attribute" that defines a dropdown on the UI
    public bool IsVariantDefiner { get; private set; }

    public string? ValueText { get; private set; }
    public int? ValueInteger { get; private set; }
    public decimal? ValueDecimal { get; private set; }
    public Guid? ValueOptionId { get; private set; }
    public bool? ValueBoolean { get; private set; }
    public DateTime? ValueDateTime { get; private set; }

    private ProductAttributeValue(
        Guid id,
        Guid attributeId,
        Guid? productId,
        Guid? variantId,
        bool? isVariantDefiner,
        string? textValue,
        int? integerValue,
        decimal? decimalValue,
        bool? booleanValue,
        DateTime? dateTimeValue,
        Guid? optionId
    )
        : base(id)
    {
        ProductId = productId;
        VariantId = variantId;
        IsVariantDefiner = isVariantDefiner ?? false;
        AttributeId = attributeId;
        ValueText = textValue;
        ValueInteger = integerValue;
        ValueDecimal = decimalValue;
        ValueBoolean = booleanValue;
        ValueDateTime = dateTimeValue;
        ValueOptionId = optionId;
    }

    private ProductAttributeValue() { }

    internal static ProductAttributeValue Create(
        Guid id,
        Guid attributeId,
        Guid? productId,
        Guid? variantId,
        bool? isVariantDefiner,
        AttributeInputType inputType,
        string? textValue = null,
        int? integerValue = null,
        decimal? decimalValue = null,
        bool? booleanValue = null,
        DateTime? dateTimeValue = null,
        Guid? optionId = null
    )
    {
        AttributeInputTypeValidator.Validate(
            inputType,
            textValue,
            integerValue,
            decimalValue,
            booleanValue,
            dateTimeValue,
            optionId
        );
        return new ProductAttributeValue(
            id,
            attributeId,
            productId,
            variantId,
            isVariantDefiner,
            textValue,
            integerValue,
            decimalValue,
            booleanValue,
            dateTimeValue,
            optionId
        );
    }

    internal void UpdateValue(
        AttributeInputType inputType,
        string? textValue = null,
        int? integerValue = null,
        decimal? decimalValue = null,
        bool? booleanValue = null,
        DateTime? dateTimeValue = null,
        Guid? optionId = null
    )
    {
        AttributeInputTypeValidator.Validate(
            inputType,
            textValue,
            integerValue,
            decimalValue,
            booleanValue,
            dateTimeValue,
            optionId
        );
        ValueText = textValue;
        ValueInteger = integerValue;
        ValueDecimal = decimalValue;
        ValueBoolean = booleanValue;
        ValueDateTime = dateTimeValue;
        ValueOptionId = optionId;
        UpdateLastModified();
    }
}
