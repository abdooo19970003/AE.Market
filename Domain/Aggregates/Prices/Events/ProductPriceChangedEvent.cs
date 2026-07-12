using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.ValueObjects;

namespace AE.Market.Domain.Aggregates.Prices.Events;

public sealed record ProductPriceChangedDomainEvent(
    Guid PriceId,
    Guid VariantId,
    Money OldAmount,
    Money NewAmount,
    PriceType Type
) : IDomainEvent;
