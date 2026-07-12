using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Inventory.DTOs;

namespace AE.Market.Application.Features.Inventory.Commands.UpdateInventoryItem;

public sealed record UpdateInventoryItemCommand(
    Guid VariantId,
    int? LowStockThreshold,
    bool? AllowBackorder,
    int? BackorderLimit,
    ShippingDimensionsDto? ShippingDimensions
) : ICommand;
