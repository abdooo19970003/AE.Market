using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserEnabled;

internal sealed class UserEnabledEventHandler : INotificationHandler<DomainEventNotification<UserEnabledDomainEvent>>
{
    public Task Handle(DomainEventNotification<UserEnabledDomainEvent> notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
