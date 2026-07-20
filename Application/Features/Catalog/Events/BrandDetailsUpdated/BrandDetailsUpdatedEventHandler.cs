using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.BrandDetailsUpdated;

internal sealed class BrandDetailsUpdatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<BrandDetailsUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandDetailsUpdatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.BrandById(notification.DomainEvent.BrandId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.BrandsList(1, 20), cancellationToken);
    }
}
