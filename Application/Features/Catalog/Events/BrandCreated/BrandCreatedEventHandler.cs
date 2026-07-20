using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.BrandCreated;

internal sealed class BrandCreatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<BrandCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<BrandCreatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.BrandsList(1, 20), cancellationToken);
    }
}
