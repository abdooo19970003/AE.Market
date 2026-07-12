using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Cart.Events;
using MediatR;

namespace AE.Market.Application.Features.Cart.Events;

internal sealed class CartMergedHandler
    : INotificationHandler<DomainEventNotification<CartMergedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<CartMergedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        return Task.CompletedTask;
    }
}
