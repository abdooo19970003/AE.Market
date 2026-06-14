using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.GroupUnitUnitRemoved;

internal sealed class GroupUnitUnitRemovedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<GroupUnitUnitRemovedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<GroupUnitUnitRemovedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.GroupUnitById(notification.DomainEvent.GroupUnitId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.GroupUnitsList, cancellationToken);
    }
}
