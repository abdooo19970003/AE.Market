using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record BundleItemAddedDomainEvent(Guid BundleId, Guid BundleItemId, Guid ItemId, int Quantity) : IDomainEvent;

