using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.BrandDeleted;

internal sealed class BrandDeletedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<BrandDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandDeletedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.BrandById(notification.DomainEvent.BrandId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.BrandsList(1, 20), cancellationToken);
    }
}
