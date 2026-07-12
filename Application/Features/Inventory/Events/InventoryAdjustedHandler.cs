using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class InventoryAdjustedHandler(
    IRepository<Product> productRepo
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
    }
}
