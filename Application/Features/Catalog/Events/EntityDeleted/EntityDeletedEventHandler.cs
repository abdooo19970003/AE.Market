using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.EntityDeleted;

internal sealed class EntityDeletedEventHandler
    : INotificationHandler<DomainEventNotification<EntityDeletedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<EntityDeletedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
