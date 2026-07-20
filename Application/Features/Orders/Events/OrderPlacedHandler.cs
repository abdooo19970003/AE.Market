using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Orders.Events;
using MediatR;
using CartCacheKeys = AE.Market.Application.Features.Cart.CacheKeys;

namespace AE.Market.Application.Features.Orders.Events;

internal sealed class OrderPlacedHandler(
    IStockManager stockManager,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<OrderPlacedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderPlacedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        foreach (var (variantId, quantity) in evt.Items)
        {
            await stockManager.ReserveStockAsync(variantId, quantity, cancellationToken);
        }

        await cache.RemoveAsync(CartCacheKeys.CartByUser(evt.UserId), cancellationToken);
    }
}
