using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Inventory.Commands.ReserveStock;

public sealed record ReserveStockCommand(
    Guid VariantId,
    int Quantity
) : ICommand;
