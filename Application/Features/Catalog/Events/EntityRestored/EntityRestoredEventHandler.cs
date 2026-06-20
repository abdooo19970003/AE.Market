using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.EntityRestored;

internal sealed class EntityRestoredEventHandler
    : INotificationHandler<DomainEventNotification<EntityRestoredDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<EntityRestoredDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
