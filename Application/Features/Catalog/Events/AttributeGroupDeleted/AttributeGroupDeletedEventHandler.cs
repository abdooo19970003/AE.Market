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
        await cache.RemoveAsync(CacheKeys.AttributeGroupsList, cancellationToken);
    }
}
