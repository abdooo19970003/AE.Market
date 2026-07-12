using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.AttributeGroupUpdated;

internal sealed class AttributeGroupUpdatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<AttributeGroupUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<AttributeGroupUpdatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.AttributeGroupsList, cancellationToken);
    }
}
