using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductDetailsUpdatedDomainEvent(Guid ProductId, string Name, string Details) : IDomainEvent;
