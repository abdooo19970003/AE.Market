using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserLoggedOut;

internal sealed class UserLoggedOutEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<UserLoggedOutDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserLoggedOutDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.UserId(evt.UserId), cancellationToken);
    }
}
