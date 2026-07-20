using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductCategoryChanged;

internal sealed class ProductCategoryChangedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductCategoryChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductCategoryChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ProductById(evt.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsByCategory(evt.OldCategoryId, 1, 20), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsByCategory(evt.NewCategoryId, 1, 20), cancellationToken);
    }
}
