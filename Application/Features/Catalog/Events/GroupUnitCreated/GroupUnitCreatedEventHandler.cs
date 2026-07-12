using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.GroupUnitCreated;

internal sealed class GroupUnitCreatedEventHandler
    : INotificationHandler<DomainEventNotification<GroupUnitCreatedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<GroupUnitCreatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
