using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record AttributeGroupDeletedDomainEvent(Guid AttributeGroupId, Guid CategoryId) : IDomainEvent;
