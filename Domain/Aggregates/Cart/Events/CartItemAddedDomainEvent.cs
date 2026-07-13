using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Cart.Events;

public sealed record CartItemAddedDomainEvent(Guid CartId, Guid VariantId, int Quantity) : IDomainEvent;
