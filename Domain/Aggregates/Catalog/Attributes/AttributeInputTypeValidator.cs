using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Catalog.Attributes;

public static class AttributeInputTypeValidator
{
    public static void Validate(AttributeInputType inputType,
        string? textValue, int? integerValue, decimal? decimalValue,
        bool? booleanValue, DateTime? dateTimeValue, Guid? optionId)
    {
        bool isMismatch = inputType switch
        {
            AttributeInputType.Text when textValue is null => true,
            AttributeInputType.Text when (integerValue, decimalValue, booleanValue, dateTimeValue, optionId) is not (null, null, null, null, null) => true,
            AttributeInputType.Integer when integerValue is null => true,
            AttributeInputType.Integer when (textValue, decimalValue, booleanValue, dateTimeValue, optionId) is not (null, null, null, null, null) => true,
            AttributeInputType.Decimal when decimalValue is null => true,
            AttributeInputType.Decimal when (textValue, integerValue, booleanValue, dateTimeValue, optionId) is not (null, null, null, null, null) => true,
            AttributeInputType.MultiSelect when optionId is null => true,
            AttributeInputType.MultiSelect when (textValue, integerValue, decimalValue, booleanValue, dateTimeValue) is not (null, null, null, null, null) => true,
            AttributeInputType.Boolean when booleanValue is null => true,
            AttributeInputType.Boolean when (textValue, integerValue, decimalValue, dateTimeValue, optionId) is not (null, null, null, null, null) => true,
            AttributeInputType.DateTime when dateTimeValue is null => true,
            AttributeInputType.DateTime when (textValue, integerValue, decimalValue, booleanValue, optionId) is not (null, null, null, null, null) => true,
            _ => false
        };

        if (isMismatch)
            throw new DomainException(
                CatalogErrors.AttributeValueTypeMismatch.Code,
                CatalogErrors.AttributeValueTypeMismatch.Message);
    }
}
