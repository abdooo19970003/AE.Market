using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Analytics.Events;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Events;

// No-op handler: ViewCount is already incremented by RecordProductViewHandler.
// This handler exists so the domain event is acknowledged by the outbox pipeline
// and doesn't cause "unhandled notification" warnings.
internal sealed class ProductViewedEventHandler
    : INotificationHandler<DomainEventNotification<ProductViewedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<ProductViewedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
