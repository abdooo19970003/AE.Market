using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.AdjustVariantStock;

public sealed record AdjustVariantStockCommand(
    Guid ProductId,
    Guid VariantId,
    int Delta
) : ICommand<VariantDto>;
