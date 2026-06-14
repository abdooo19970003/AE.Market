using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Inventory.Events;

public sealed record OutOfStockDomainEvent(
    Guid InventoryItemId,
    Guid VariantId
) : IDomainEvent;
