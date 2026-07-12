using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record VariantPriceChangedDomainEvent(Guid ProductId, Guid VariantId, decimal OldPrice, decimal NewPrice) : IDomainEvent;
