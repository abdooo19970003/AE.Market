using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record GroupUnitUnitRemovedDomainEvent(Guid GroupUnitId, Guid UnitId) : IDomainEvent;
