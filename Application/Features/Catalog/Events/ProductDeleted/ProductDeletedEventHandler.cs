using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductDeleted;

internal sealed class ProductDeletedEventHandler(
    ICacheService cache,
    IReadRepository<Product> productRepo
) : INotificationHandler<DomainEventNotification<ProductDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductDeletedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var product = await productRepo.GetByIdAsync(notification.DomainEvent.ProductId, cancellationToken);
        if (product is null)
            return;

        var id = notification.DomainEvent.ProductId;
        await cache.RemoveAsync(CacheKeys.ProductById(id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductBySlug(product.Slug), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList, cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsByCategory(product.CategoryId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsByBrand(product.BrandId), cancellationToken);
    }
}
