using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductUpdated;

internal sealed class ProductUpdatedEventHandler(
    ICacheService cache,
    IReadRepository<Product> productRepo
) : INotificationHandler<DomainEventNotification<ProductUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductUpdatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null)
            return;

        var id = notification.DomainEvent.ProductId;
        await cache.RemoveAsync(CacheKeys.ProductById(id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductBySlug(product.Slug), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsByCategory(product.CategoryId, 1, 20), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsByBrand(product.BrandId, 1, 20), cancellationToken);
    }
}
