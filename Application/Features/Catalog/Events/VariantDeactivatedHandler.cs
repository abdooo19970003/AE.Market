using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events;

internal sealed class VariantDeactivatedHandler(
    IRepository<Product> productRepo,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<VariantDeactivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantDeactivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var product = await productRepo.GetBySpecWithTrackingAsync(
            new ProductByVariantIdSpec(evt.VariantId),
            cancellationToken);

        if (product is null)
            return;

        var hasActiveVariant = product.Variants.Any(v =>
            v.Status == ProductStatus.Active && !v.IsDeleted);

        if (!hasActiveVariant && product.Status == ProductStatus.Active)
        {
            product.Deactivate();
        }

        await cache.RemoveAsync(CacheKeys.ProductById(evt.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
    }
}
