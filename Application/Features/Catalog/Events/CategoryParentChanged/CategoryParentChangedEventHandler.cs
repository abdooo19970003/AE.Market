using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.CategoryParentChanged;

internal sealed class CategoryParentChangedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<CategoryParentChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CategoryParentChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.CategoryById(notification.DomainEvent.CategoryId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.CategoriesList, cancellationToken);
    }
}
