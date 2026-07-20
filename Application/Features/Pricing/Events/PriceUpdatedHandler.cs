using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Prices.Events;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Events;

internal sealed class PriceUpdatedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<PriceUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PriceUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ActivePrice(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.PriceHistory(evt.VariantId), cancellationToken);
    }
}
