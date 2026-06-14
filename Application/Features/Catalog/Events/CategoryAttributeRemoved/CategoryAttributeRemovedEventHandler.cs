using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.CategoryAttributeRemoved;

internal sealed class CategoryAttributeRemovedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<CategoryAttributeRemovedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CategoryAttributeRemovedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.CategoryById(notification.DomainEvent.CategoryId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.CategoriesList, cancellationToken);
    }
}
