using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Orders.Events;
using MediatR;

namespace AE.Market.Application.Features.Orders.Events;

internal sealed class OrderPlacedHandler(
    IStockManager stockManager
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
    }
}
