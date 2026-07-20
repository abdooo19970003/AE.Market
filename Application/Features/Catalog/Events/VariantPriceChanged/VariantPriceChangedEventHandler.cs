using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;
using PricingCacheKeys = AE.Market.Application.Features.Pricing.CacheKeys;

namespace AE.Market.Application.Features.Catalog.Events.VariantPriceChanged;

internal sealed class VariantPriceChangedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<VariantPriceChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantPriceChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ProductById(evt.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList(1, 20), cancellationToken);
        await cache.RemoveAsync(PricingCacheKeys.ActivePrice(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(PricingCacheKeys.PriceHistory(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(PricingCacheKeys.Margin(evt.VariantId), cancellationToken);
    }
}
