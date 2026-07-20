using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductTaxCodeCreated;

internal sealed class ProductTaxCodeCreatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductTaxCodeCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductTaxCodeCreatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.ProductTaxCodesList(1, 20), cancellationToken);
    }
}
