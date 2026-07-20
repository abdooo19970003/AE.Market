using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Orders.Specs;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Aggregates.Orders.Events;
using MediatR;

namespace AE.Market.Application.Features.Orders.Events;

internal sealed class OrderCancelledHandler(
    IReadRepository<Order> orderRepo,
    IStockManager stockManager,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<OrderCancelledDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCancelledDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var order = await orderRepo.FirstOrDefaultAsync(
            new OrderByIdSpec(evt.OrderId), cancellationToken);

        if (order is null)
            return;

        foreach (var item in order.Items)
        {
            await stockManager.ReleaseStockAsync(item.VariantId, item.Quantity, cancellationToken);
        }

        await cache.RemoveAsync(CacheKeys.Order(evt.OrderId), cancellationToken);
    }
}
