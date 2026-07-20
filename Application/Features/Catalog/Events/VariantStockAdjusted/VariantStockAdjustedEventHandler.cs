using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;
using InventoryCacheKeys = AE.Market.Application.Features.Inventory.CacheKeys;

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
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ProductById(evt.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
        await cache.RemoveAsync(InventoryCacheKeys.Stock(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(InventoryCacheKeys.LowStockReport, cancellationToken);
    }
}
