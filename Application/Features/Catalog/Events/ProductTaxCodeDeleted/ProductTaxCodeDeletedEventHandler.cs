using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.ProductTaxCodeDeleted;

internal sealed class ProductTaxCodeDeletedEventHandler
    : INotificationHandler<DomainEventNotification<ProductTaxCodeDeletedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<ProductTaxCodeDeletedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
