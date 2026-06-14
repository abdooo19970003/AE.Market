using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Prices.Events;

public sealed record PriceDeletedDomainEvent(
    Guid PriceId,
    Guid VariantId
) : IDomainEvent;
