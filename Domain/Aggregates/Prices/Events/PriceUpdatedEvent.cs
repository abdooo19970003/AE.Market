using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Prices.Events;

public sealed record PriceUpdatedDomainEvent(
    Guid PriceId,
    Guid VariantId
) : IDomainEvent;
