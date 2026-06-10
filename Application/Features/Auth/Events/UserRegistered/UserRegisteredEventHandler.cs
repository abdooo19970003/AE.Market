using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserRegistered;

internal sealed class UserRegisteredEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<UserRegisteredDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserRegisteredDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(
            CacheKeys.UserId(notification.DomainEvent.UserId),
            cancellationToken
        );
    }
}
