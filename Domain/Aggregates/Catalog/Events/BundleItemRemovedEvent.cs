using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record BundleItemRemovedDomainEvent(Guid BundleId, Guid BundleItemId) : IDomainEvent;

