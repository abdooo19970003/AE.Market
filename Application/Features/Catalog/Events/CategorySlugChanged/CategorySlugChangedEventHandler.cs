using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.CategorySlugChanged;

internal sealed class CategorySlugChangedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<CategorySlugChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CategorySlugChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.CategoryById(notification.DomainEvent.CategoryId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.CategoriesList(1, 20), cancellationToken);
    }
}
