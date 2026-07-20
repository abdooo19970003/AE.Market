using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.GroupUnitRenamed;

internal sealed class GroupUnitRenamedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<GroupUnitRenamedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<GroupUnitRenamedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.GroupUnitById(notification.DomainEvent.GroupUnitId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.GroupUnitsList(1, 20), cancellationToken);
    }
}
