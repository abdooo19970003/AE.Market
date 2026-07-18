using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class StockReservedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<StockReservedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<StockReservedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.Stock(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.LowStockReport, cancellationToken);
    }
}
