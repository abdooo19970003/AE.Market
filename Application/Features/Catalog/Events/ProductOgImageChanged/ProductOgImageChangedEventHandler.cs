using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductOgImageChanged;

internal sealed class ProductOgImageChangedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductOgImageChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductOgImageChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var productId = notification.DomainEvent.ProductId;
        await cache.RemoveAsync(CacheKeys.ProductById(productId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
    }
}
