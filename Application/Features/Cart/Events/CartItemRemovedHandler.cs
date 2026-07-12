using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Cart.Events;
using MediatR;

namespace AE.Market.Application.Features.Cart.Events;

internal sealed class CartItemRemovedHandler
    : INotificationHandler<DomainEventNotification<CartItemRemovedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<CartItemRemovedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        return Task.CompletedTask;
    }
}
