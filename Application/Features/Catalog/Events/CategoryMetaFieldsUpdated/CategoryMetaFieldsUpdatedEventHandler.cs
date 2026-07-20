using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.CategoryMetaFieldsUpdated;

internal sealed class CategoryMetaFieldsUpdatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<CategoryMetaFieldsUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CategoryMetaFieldsUpdatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.CategoryById(notification.DomainEvent.CategoryId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.CategoriesList(1, 20), cancellationToken);
    }
}
