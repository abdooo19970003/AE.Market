using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Prices.Events;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Events;

internal sealed class PriceDeletedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<PriceDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PriceDeletedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ActivePrice(evt.VariantId), cancellationToken);
    }
}
