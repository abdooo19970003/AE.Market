using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record CategoryAttributeAddedDomainEvent(Guid CategoryId, Guid AttributeId, string AttributeName) : IDomainEvent;
