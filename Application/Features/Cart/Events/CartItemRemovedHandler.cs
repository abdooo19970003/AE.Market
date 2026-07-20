using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Cart.Events;
using MediatR;
using CartEntity = AE.Market.Domain.Aggregates.Cart.Cart;

namespace AE.Market.Application.Features.Cart.Events;

internal sealed class CartItemRemovedHandler(
    ICacheService cache,
    IRepository<CartEntity> cartRepo
) : INotificationHandler<DomainEventNotification<CartItemRemovedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CartItemRemovedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var cart = await cartRepo.GetByIdAsync(evt.CartId, cancellationToken);
        if (cart is null)
            return;

        if (cart.UserId.HasValue)
            await cache.RemoveAsync(CacheKeys.CartByUser(cart.UserId.Value), cancellationToken);
        if (cart.SessionId.HasValue)
            await cache.RemoveAsync(CacheKeys.CartBySession(cart.SessionId.Value), cancellationToken);
    }
}
