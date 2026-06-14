using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductSlugChangedDomainEvent(Guid ProductId, string OldSlug, string NewSlug) : IDomainEvent;
