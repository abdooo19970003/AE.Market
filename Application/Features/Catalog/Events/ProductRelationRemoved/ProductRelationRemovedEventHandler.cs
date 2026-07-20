using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductRelationRemoved;

internal sealed class ProductRelationRemovedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductRelationRemovedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductRelationRemovedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var id = notification.DomainEvent.ProductId;
        await cache.RemoveAsync(CacheKeys.ProductById(id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
    }
}
