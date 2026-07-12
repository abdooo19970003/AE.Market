using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Inventory.Events;

public sealed record LowStockThresholdReachedDomainEvent(
    Guid InventoryItemId,
    Guid VariantId,
    int CurrentStock,
    int Threshold
) : IDomainEvent;
