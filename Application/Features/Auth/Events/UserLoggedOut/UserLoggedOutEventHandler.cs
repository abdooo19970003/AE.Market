using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserLoggedOut;

internal sealed class UserLoggedOutEventHandler : INotificationHandler<DomainEventNotification<UserLoggedOutDomainEvent>>
{
    public Task Handle(DomainEventNotification<UserLoggedOutDomainEvent> notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
