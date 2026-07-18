using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events;

internal sealed class VariantActivatedHandler(
    IRepository<Product> productRepo,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<VariantActivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantActivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var product = await productRepo.GetByIdWithTrackingAsync(evt.ProductId, cancellationToken);
        if (product is null)
            return;

        if (product.Status != ProductStatus.Active)
        {
            product.Activate();
        }

        await cache.RemoveAsync(CacheKeys.ProductById(evt.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList, cancellationToken);
    }
}
