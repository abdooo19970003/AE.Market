using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.AttributeGroupDeleted;

internal sealed class AttributeGroupDeletedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<AttributeGroupDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<AttributeGroupDeletedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.AttributeGroupsList(1, 20), cancellationToken);
        await cache.RemoveAsync(CacheKeys.AttributeGroupById(evt.AttributeGroupId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.AttributeGroupsByCategory(evt.CategoryId), cancellationToken);
    }
}
