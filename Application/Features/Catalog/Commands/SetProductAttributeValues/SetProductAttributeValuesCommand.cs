using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.SetProductAttributeValues;

public sealed record SetProductAttributeValuesCommand(
    Guid ProductId,
    IReadOnlyCollection<AttributeValueDto> AttributeValues
) : ICommand;
