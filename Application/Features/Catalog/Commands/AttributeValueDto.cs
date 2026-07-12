using AE.Market.Domain.Aggregates.Catalog.Attributes;

namespace AE.Market.Application.Features.Catalog.Commands;

public sealed record AttributeValueDto(
    Guid AttributeId,
    AttributeInputType InputType,
    bool? IsVariantDefiner = null,
    string? ValueText = null,
    int? ValueInteger = null,
    decimal? ValueDecimal = null,
    bool? ValueBoolean = null,
    DateTime? ValueDateTime = null,
    Guid? OptionId = null
);
