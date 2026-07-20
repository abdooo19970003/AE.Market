using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class InventoryAdjustedHandler(
    IRepository<Product> productRepo,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<StockAdjustedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<StockAdjustedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var evt = notification.DomainEvent;

        var product = await productRepo.GetBySpecWithTrackingAsync(
            new ProductByVariantIdSpec(evt.VariantId),
            cancellationToken);

        if (product is null)
            return;

        var variant = product.Variants.FirstOrDefault(v => v.Id == evt.VariantId);
        if (variant is null)
            return;

        variant.SetQuantity(evt.NewQuantity);

        await cache.RemoveAsync(Features.Inventory.CacheKeys.Stock(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(Features.Inventory.CacheKeys.LowStockReport, cancellationToken);
        await cache.RemoveAsync(Features.Catalog.CacheKeys.ProductById(product.Id), cancellationToken);
        await cache.RemoveAsync(Features.Catalog.CacheKeys.ProductsList(1, 20), cancellationToken);
    }
}
