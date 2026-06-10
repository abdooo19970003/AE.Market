using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserLoggedIn;

internal sealed class UserLoggedInEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<UserLoggedInDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserLoggedInDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(
            CacheKeys.UserId(notification.DomainEvent.Id),
            cancellationToken
        );
    }
}
