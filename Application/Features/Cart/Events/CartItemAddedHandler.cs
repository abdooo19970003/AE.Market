using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Cart.Events;
using MediatR;

namespace AE.Market.Application.Features.Cart.Events;

internal sealed class CartItemAddedHandler
    : INotificationHandler<DomainEventNotification<CartItemAddedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<CartItemAddedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        return Task.CompletedTask;
    }
}
