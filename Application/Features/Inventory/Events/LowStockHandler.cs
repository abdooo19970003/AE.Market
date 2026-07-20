using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class LowStockHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<LowStockThresholdReachedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LowStockThresholdReachedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CacheKeys.LowStockReport, cancellationToken);
    }
}
