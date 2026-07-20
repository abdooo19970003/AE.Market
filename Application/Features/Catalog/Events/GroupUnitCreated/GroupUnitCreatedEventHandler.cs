using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.GroupUnitCreated;

internal sealed class GroupUnitCreatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<GroupUnitCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<GroupUnitCreatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.GroupUnitsList(1, 20), cancellationToken);
    }
}
