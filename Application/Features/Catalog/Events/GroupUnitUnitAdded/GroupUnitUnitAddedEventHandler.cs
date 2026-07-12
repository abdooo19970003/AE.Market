using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.GroupUnitUnitAdded;

internal sealed class GroupUnitUnitAddedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<GroupUnitUnitAddedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<GroupUnitUnitAddedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.GroupUnitById(notification.DomainEvent.GroupUnitId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.GroupUnitsList, cancellationToken);
    }
}
