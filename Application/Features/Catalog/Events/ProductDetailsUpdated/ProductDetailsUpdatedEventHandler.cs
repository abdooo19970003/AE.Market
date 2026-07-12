using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductDetailsUpdated;

internal sealed class ProductDetailsUpdatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductDetailsUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductDetailsUpdatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var id = notification.DomainEvent.ProductId;
        await cache.RemoveAsync(CacheKeys.ProductById(id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList, cancellationToken);
    }
}
