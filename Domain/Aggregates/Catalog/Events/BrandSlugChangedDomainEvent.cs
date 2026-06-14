using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record BrandSlugChangedDomainEvent(Guid BrandId, string OldSlug, string NewSlug) : IDomainEvent;
