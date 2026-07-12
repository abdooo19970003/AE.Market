using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Inventory.Commands.AdjustStock;

public sealed record AdjustStockCommand(
    Guid VariantId,
    int Delta
) : ICommand;
