using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserProfileCreated;

internal sealed class UserProfileCreatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<UserProfileCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserProfileCreatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(
            CacheKeys.UserId(notification.DomainEvent.UserId),
            cancellationToken
        );
    }
}
