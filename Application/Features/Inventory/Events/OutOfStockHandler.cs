using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class OutOfStockHandler(
    IRepository<ProductVariant> variantRepo,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<OutOfStockDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OutOfStockDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var variant = await variantRepo.GetByIdWithTrackingAsync(evt.VariantId, cancellationToken);
        if (variant is not null)
        {
            variant.Deactivate();
        }

        await cache.RemoveAsync(CacheKeys.Stock(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.LowStockReport, cancellationToken);
    }
}
