using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserProfileUpdated;

internal sealed class UserProfileUpdatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<UserProfileUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserProfileUpdatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(
            CacheKeys.UserId(notification.DomainEvent.UserId),
            cancellationToken
        );
    }
}
