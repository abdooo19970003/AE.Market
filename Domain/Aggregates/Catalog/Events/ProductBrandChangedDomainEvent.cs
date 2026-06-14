using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductBrandChangedDomainEvent(Guid ProductId, Guid OldBrandId, Guid NewBrandId) : IDomainEvent;
