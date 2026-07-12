using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductShortDescriptionChangedDomainEvent(Guid ProductId, string? ShortDescription) : IDomainEvent;
