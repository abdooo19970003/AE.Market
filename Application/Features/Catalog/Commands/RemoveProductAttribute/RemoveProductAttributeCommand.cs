using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductAttribute;

public sealed record RemoveProductAttributeCommand(
    Guid ProductId,
    Guid AttributeValueId
) : ICommand;
