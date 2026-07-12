using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductTagAddedDomainEvent(Guid ProductId, Guid TagId, string Name, string TagSlug) : IDomainEvent;
