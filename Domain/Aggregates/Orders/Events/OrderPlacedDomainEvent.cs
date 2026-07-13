using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Orders.Events;

public sealed record OrderPlacedDomainEvent(
    Guid OrderId,
    Guid UserId,
    IReadOnlyList<(Guid VariantId, int Quantity)> Items) : IDomainEvent;
