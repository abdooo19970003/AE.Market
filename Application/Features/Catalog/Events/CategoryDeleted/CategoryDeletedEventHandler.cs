using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.CategoryDeleted;

internal sealed class CategoryDeletedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<CategoryDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CategoryDeletedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.CategoryById(notification.DomainEvent.CategoryId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.CategoriesList, cancellationToken);
    }
}
