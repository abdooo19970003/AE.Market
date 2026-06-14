using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.GroupUnitDeleted;

internal sealed class GroupUnitDeletedEventHandler
    : INotificationHandler<DomainEventNotification<GroupUnitDeletedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<GroupUnitDeletedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
