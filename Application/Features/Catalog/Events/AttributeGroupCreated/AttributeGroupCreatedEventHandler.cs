using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.AttributeGroupCreated;

internal sealed class AttributeGroupCreatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<AttributeGroupCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<AttributeGroupCreatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.AttributeGroupsList(1, 20), cancellationToken);
    }
}
