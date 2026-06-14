using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record CategoryParentChangedDomainEvent(Guid CategoryId, Guid? OldParentId, Guid? NewParentId) : IDomainEvent;
