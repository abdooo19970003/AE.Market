using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Inventory.DTOs;

namespace AE.Market.Application.Features.Inventory.Commands.CreateInventoryItem;

public sealed record CreateInventoryItemCommand(
    Guid VariantId,
    Guid WarehouseId,
    int StockQuantity = 0,
    bool TrackInventory = true,
    bool AllowBackorder = false,
    int? BackorderLimit = null,
    int LowStockThreshold = 0
) : ICommand<InventoryItemDto>;
