using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class InventoryCreatedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<InventoryCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<InventoryCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.Stock(evt.VariantId), cancellationToken);
    }
}
