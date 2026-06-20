using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.BundleItemRemoved;

internal sealed class BundleItemRemovedEventHandler
    : INotificationHandler<DomainEventNotification<BundleItemRemovedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<BundleItemRemovedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
