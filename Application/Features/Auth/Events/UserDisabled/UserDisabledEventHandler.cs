using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth.Events;
using MediatR;

namespace AE.Market.Application.Features.Auth.Events.UserDisabled;

internal sealed class UserDisabledEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<UserDisabledDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserDisabledDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.UserId(evt.UserId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.UsersList(1, 20), cancellationToken);
    }
}
