using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductRelationRemovedDomainEvent(Guid ProductId, Guid RelatedProductId, RelationType Type) : IDomainEvent;
