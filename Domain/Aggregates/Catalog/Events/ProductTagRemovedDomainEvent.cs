using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductTagRemovedDomainEvent(Guid ProductId, string TagSlug) : IDomainEvent;
