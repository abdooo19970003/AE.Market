using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.ReleaseVariantStock;

public sealed record ReleaseVariantStockCommand(
    Guid ProductId,
    Guid VariantId,
    int Quantity
) : ICommand<VariantDto>;
