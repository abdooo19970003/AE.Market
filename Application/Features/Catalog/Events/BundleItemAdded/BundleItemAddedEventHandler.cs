using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.BundleItemAdded;

internal sealed class BundleItemAddedEventHandler
    : INotificationHandler<DomainEventNotification<BundleItemAddedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<BundleItemAddedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
