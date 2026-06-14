using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.BrandActivated;

internal sealed class BrandActivatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<BrandActivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandActivatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.BrandById(notification.DomainEvent.BrandId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.BrandsList, cancellationToken);
    }
}
