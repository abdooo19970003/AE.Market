using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductTaxCodeUpdated;

internal sealed class ProductTaxCodeUpdatedEventHandler
    : INotificationHandler<DomainEventNotification<ProductTaxCodeUpdatedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<ProductTaxCodeUpdatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
