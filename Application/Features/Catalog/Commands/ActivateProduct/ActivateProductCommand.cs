using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Catalog.Commands.ActivateProduct;

public sealed record ActivateProductCommand(
    Guid ProductId
) : ICommand;
