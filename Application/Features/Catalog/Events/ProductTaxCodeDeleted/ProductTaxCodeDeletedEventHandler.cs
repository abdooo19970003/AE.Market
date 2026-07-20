using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductTaxCodeDeleted;

internal sealed class ProductTaxCodeDeletedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductTaxCodeDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductTaxCodeDeletedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ProductTaxCodesList(1, 20), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductTaxCodeById(evt.TaxCodeId), cancellationToken);
    }
}
