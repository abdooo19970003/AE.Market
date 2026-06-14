using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record VariantStockAdjustedDomainEvent(Guid ProductId, Guid VariantId, int OldQuantity, int NewQuantity, int Delta) : IDomainEvent;
