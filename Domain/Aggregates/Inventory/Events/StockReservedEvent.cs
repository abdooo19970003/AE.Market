using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Inventory.Events;

public sealed record StockReservedDomainEvent(
    Guid InventoryItemId,
    Guid VariantId,
    int Quantity,
    int NewReservedTotal
) : IDomainEvent;
