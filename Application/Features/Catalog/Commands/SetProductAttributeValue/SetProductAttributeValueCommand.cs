using AE.Market.Application.Common.Abstracts;
using AE.Market.Domain.Aggregates.Catalog.Attributes;

namespace AE.Market.Application.Features.Catalog.Commands.SetProductAttributeValue;

public sealed record SetProductAttributeValueCommand(
    Guid ProductId,
    Guid AttributeId,
    AttributeInputType InputType,
    bool? IsVariantDefiner,
    string? ValueText = null,
    int? ValueInteger = null,
    decimal? ValueDecimal = null,
    bool? ValueBoolean = null,
    DateTime? ValueDateTime = null,
    Guid? OptionId = null
) : ICommand;
