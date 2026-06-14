using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateVariantStock;

public sealed record UpdateVariantStockCommand(
    Guid ProductId,
    Guid VariantId,
    int StockQuantity
) : ICommand<VariantDto>;
