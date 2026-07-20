using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.BundleItemAdded;

internal sealed class BundleItemAddedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<BundleItemAddedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BundleItemAddedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.BundleItemsList(1, 20), cancellationToken);
        await cache.RemoveAsync(CacheKeys.BundleItemsByBundle(evt.BundleId), cancellationToken);
    }
}
