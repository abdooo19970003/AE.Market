using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductTagRemoved;

internal sealed class ProductTagRemovedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductTagRemovedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductTagRemovedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var id = notification.DomainEvent.ProductId;
        await cache.RemoveAsync(CacheKeys.ProductById(id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
    }
}
