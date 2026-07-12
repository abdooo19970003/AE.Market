using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Cart.Events;

public sealed record CartItemRemovedDomainEvent(Guid CartId, Guid VariantId) : IDomainEvent;
