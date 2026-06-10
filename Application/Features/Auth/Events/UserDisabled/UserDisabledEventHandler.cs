using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserDisabled;

internal sealed class UserDisabledEventHandler : INotificationHandler<DomainEventNotification<UserDisabledDomainEvent>>
{
    public Task Handle(DomainEventNotification<UserDisabledDomainEvent> notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
