using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductTaxCodeCreated;

internal sealed class ProductTaxCodeCreatedEventHandler
    : INotificationHandler<DomainEventNotification<ProductTaxCodeCreatedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<ProductTaxCodeCreatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
