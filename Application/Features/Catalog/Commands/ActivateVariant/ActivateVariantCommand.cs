using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.ActivateVariant;

public sealed record ActivateVariantCommand(
    Guid ProductId,
    Guid VariantId
) : ICommand;
