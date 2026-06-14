using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.VariantStockAdjusted;

internal sealed class VariantStockAdjustedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<VariantStockAdjustedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantStockAdjustedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.ProductById(notification.DomainEvent.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList, cancellationToken);
    }
}
