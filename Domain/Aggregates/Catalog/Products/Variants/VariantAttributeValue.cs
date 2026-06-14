using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Products.Variants;

public sealed partial class VariantAttributeValue : BaseEntity
{
    public Guid VariantId { get; private set; }

    public Guid AttributeId { get; private set; }

    public string? ValueText { get; private set; }
    public int? ValueInteger { get; private set; }
    public decimal? ValueDecimal { get; private set; }
    public Guid? ValueOptionId { get; private set; }
    public bool? ValueBoolean { get; private set; }
    public DateTime? ValueDateTime { get; private set; }

    private VariantAttributeValue(Guid id, Guid variantId, Guid attributeId,
        string? textValue, int? integerValue, decimal? decimalValue,
        bool? booleanValue, DateTime? dateTimeValue, Guid? optionId)
        : base(id)
    {
        VariantId = variantId;
        AttributeId = attributeId;
        ValueText = textValue;
        ValueInteger = integerValue;
        ValueDecimal = decimalValue;
        ValueBoolean = booleanValue;
        ValueDateTime = dateTimeValue;
        ValueOptionId = optionId;
    }

    private VariantAttributeValue() { }

    internal static VariantAttributeValue Create(Guid id, Guid variantId, Guid attributeId,
        AttributeInputType inputType,
        string? textValue = null, int? integerValue = null, decimal? decimalValue = null,
        bool? booleanValue = null, DateTime? dateTimeValue = null, Guid? optionId = null)
    {
        AttributeInputTypeValidator.Validate(inputType, textValue, integerValue, decimalValue, booleanValue, dateTimeValue, optionId);
        return new VariantAttributeValue(id, variantId, attributeId, textValue, integerValue,
            decimalValue, booleanValue, dateTimeValue, optionId);
    }

    internal void UpdateValue(AttributeInputType inputType,
        string? textValue = null, int? integerValue = null,
        decimal? decimalValue = null, bool? booleanValue = null,
        DateTime? dateTimeValue = null, Guid? optionId = null)
    {
        AttributeInputTypeValidator.Validate(inputType, textValue, integerValue, decimalValue, booleanValue, dateTimeValue, optionId);
        ValueText = textValue;
        ValueInteger = integerValue;
        ValueDecimal = decimalValue;
        ValueBoolean = booleanValue;
        ValueDateTime = dateTimeValue;
        ValueOptionId = optionId;
        UpdateLastModified();
    }
}
