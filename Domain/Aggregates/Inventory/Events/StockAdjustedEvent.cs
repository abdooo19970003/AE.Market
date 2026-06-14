using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Inventory.Events;

public sealed record StockAdjustedDomainEvent(
    Guid InventoryItemId,
    Guid VariantId,
    int OldQuantity,
    int NewQuantity,
    int Delta
) : IDomainEvent;
