using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AE.Market.Application.Features.Auth.Events.RefreshTokenReused;

internal sealed class RefreshTokenReusedEventHandler(
    ICacheService cache,
    ILogger<RefreshTokenReusedEventHandler> logger
) : INotificationHandler<DomainEventNotification<RefreshTokenReusedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<RefreshTokenReusedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var domainEvent = notification.DomainEvent;

        logger.LogWarning(
            "Refresh token reuse detected for user {UserId}. Token: {UsedToken}. All refresh tokens have been revoked.",
            domainEvent.UserId,
            domainEvent.UsedToken
        );

        await cache.RemoveAsync(
            CacheKeys.UserId(domainEvent.UserId),
            cancellationToken
        );
    }
}
