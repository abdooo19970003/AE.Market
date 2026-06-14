using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.CategoryAttributeAdded;

internal sealed class CategoryAttributeAddedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<CategoryAttributeAddedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CategoryAttributeAddedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.CategoryById(notification.DomainEvent.CategoryId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.CategoriesList, cancellationToken);
    }
}
