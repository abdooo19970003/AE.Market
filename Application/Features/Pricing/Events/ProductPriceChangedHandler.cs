using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Prices.Events;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Events;

internal sealed class ProductPriceChangedHandler(
    IRepository<Product> productRepo,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<ProductPriceChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProductPriceChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var evt = notification.DomainEvent;

        var product = await productRepo.GetBySpecWithTrackingAsync(
            new ProductByVariantIdSpec(evt.VariantId),
            cancellationToken);

        if (product is null)
            return;

        if (evt.Type == PriceType.List)
            product.SetVariantListPrice(evt.VariantId, evt.NewAmount.Amount);

        await cache.RemoveAsync(Features.Catalog.CacheKeys.ProductById(product.Id), cancellationToken);
        await cache.RemoveAsync(Features.Catalog.CacheKeys.ProductsList(1, 20), cancellationToken);
    }
}
