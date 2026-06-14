using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductCategoryChangedDomainEvent(Guid ProductId, Guid OldCategoryId, Guid NewCategoryId) : IDomainEvent;
