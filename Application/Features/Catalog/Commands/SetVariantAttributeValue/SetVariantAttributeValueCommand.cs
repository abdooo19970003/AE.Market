using AE.Market.Application.Common.Abstracts;
using AE.Market.Domain.Aggregates.Catalog.Attributes;

namespace AE.Market.Application.Features.Catalog.Commands.SetVariantAttributeValue;

public sealed record SetVariantAttributeValueCommand(
    Guid ProductId,
    Guid VariantId,
    Guid AttributeId,
    AttributeInputType InputType,
    string? ValueText = null,
    int? ValueInteger = null,
    decimal? ValueDecimal = null,
    bool? ValueBoolean = null,
    DateTime? ValueDateTime = null,
    Guid? OptionId = null
) : ICommand;
