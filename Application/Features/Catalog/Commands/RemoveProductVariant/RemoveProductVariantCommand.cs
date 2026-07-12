using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.RemoveProductVariant;

public sealed record RemoveProductVariantCommand(
    Guid ProductId,
    Guid VariantId
) : ICommand;
