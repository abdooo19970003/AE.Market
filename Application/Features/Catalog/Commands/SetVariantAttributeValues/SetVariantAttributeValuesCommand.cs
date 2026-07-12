using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.SetVariantAttributeValues;

public sealed record SetVariantAttributeValuesCommand(
    Guid ProductId,
    Guid VariantId,
    IReadOnlyCollection<AttributeValueDto> AttributeValues
) : ICommand;
