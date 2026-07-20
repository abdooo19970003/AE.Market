using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.GroupUnitDeleted;

internal sealed class GroupUnitDeletedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<GroupUnitDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<GroupUnitDeletedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.GroupUnitsList(1, 20), cancellationToken);
        await cache.RemoveAsync(CacheKeys.GroupUnitById(evt.GroupUnitId), cancellationToken);
    }
}
