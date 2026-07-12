using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Inventory.Events;

public sealed record InventoryCreatedDomainEvent(
    Guid InventoryItemId,
    Guid VariantId
) : IDomainEvent;
