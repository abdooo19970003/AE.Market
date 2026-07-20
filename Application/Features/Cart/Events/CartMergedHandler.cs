using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Cart.Events;
using MediatR;

namespace AE.Market.Application.Features.Cart.Events;

internal sealed class CartMergedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<CartMergedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CartMergedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        await cache.RemoveAsync(CacheKeys.CartByUser(evt.UserId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.CartBySession(evt.GuestCartId), cancellationToken);
    }
}
