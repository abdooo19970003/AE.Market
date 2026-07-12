using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Prices.Events;

public sealed record PriceCreatedDomainEvent(
    Guid PriceId,
    Guid VariantId
) : IDomainEvent;
