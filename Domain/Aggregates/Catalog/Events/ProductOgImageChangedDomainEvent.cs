using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductOgImageChangedDomainEvent(Guid ProductId, string? OgImage) : IDomainEvent;
