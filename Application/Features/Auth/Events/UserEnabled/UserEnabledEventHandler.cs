using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserEnabled;

internal sealed class UserEnabledEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<UserEnabledDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserEnabledDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.UserId(evt.UserId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.UsersList(1, 20), cancellationToken);
    }
}
