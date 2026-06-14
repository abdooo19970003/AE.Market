using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record GroupUnitUnitAddedDomainEvent(Guid GroupUnitId, Guid UnitId, string UnitName, string Abbreviation, bool IsBaseUnit) : IDomainEvent;
