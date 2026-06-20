using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductLongDescriptionChanged;

internal sealed class ProductLongDescriptionChangedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductLongDescriptionChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductLongDescriptionChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var id = notification.DomainEvent.ProductId;
        await cache.RemoveAsync(CacheKeys.ProductById(id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList, cancellationToken);
    }
}
