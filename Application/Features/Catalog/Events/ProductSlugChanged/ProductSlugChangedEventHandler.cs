using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductSlugChanged;

internal sealed class ProductSlugChangedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductSlugChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductSlugChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ProductById(evt.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductBySlug(evt.OldSlug), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductBySlug(evt.NewSlug), cancellationToken);
    }
}
