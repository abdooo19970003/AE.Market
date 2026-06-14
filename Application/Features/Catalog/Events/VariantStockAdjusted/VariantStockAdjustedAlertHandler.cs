using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AE.Market.Application.Features.Catalog.Events.VariantStockAdjusted;

internal sealed class VariantStockAdjustedAlertHandler(
    ILogger<VariantStockAdjustedAlertHandler> logger
) : INotificationHandler<DomainEventNotification<VariantStockAdjustedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<VariantStockAdjustedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var e = notification.DomainEvent;

        if (e.NewQuantity <= 0)
        {
            logger.LogWarning(
                "Variant {VariantId} (Product {ProductId}) is out of stock. Old: {Old}, New: {New}",
                e.VariantId, e.ProductId, e.OldQuantity, e.NewQuantity);
        }
        else if (e.OldQuantity > 0 && e.NewQuantity < e.OldQuantity)
        {
            logger.LogInformation(
                "Variant {VariantId} (Product {ProductId}) stock decreased from {Old} to {New} (delta: {Delta})",
                e.VariantId, e.ProductId, e.OldQuantity, e.NewQuantity, e.Delta);
        }

        return Task.CompletedTask;
    }
}
