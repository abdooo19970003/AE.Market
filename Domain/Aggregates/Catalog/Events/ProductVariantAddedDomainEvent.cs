using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductVariantAddedDomainEvent(Guid ProductId, Guid VariantId, string VariantName, string Sku) : IDomainEvent;
