using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.VariantPriceChanged;

internal sealed class VariantPriceChangedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<VariantPriceChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantPriceChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.ProductById(notification.DomainEvent.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
    }
}
