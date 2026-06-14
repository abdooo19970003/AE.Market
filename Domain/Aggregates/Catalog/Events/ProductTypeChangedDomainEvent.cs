using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductTypeChangedDomainEvent(Guid ProductId, ProductType OldType, ProductType NewType) : IDomainEvent;
