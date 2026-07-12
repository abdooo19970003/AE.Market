using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Inventory.Commands.ReleaseStock;

public sealed record ReleaseStockCommand(
    Guid VariantId,
    int Quantity
) : ICommand;
