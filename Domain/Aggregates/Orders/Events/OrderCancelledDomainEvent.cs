using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Orders.Events;

public sealed record OrderCancelledDomainEvent(Guid OrderId, Guid UserId) : IDomainEvent;
