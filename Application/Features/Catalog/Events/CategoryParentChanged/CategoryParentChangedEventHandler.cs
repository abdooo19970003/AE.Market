using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.CategoryParentChanged;

internal sealed class CategoryParentChangedEventHandler(
    ICacheService cache,
    IReadRepository<Category> repo
) : INotificationHandler<DomainEventNotification<CategoryParentChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CategoryParentChangedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var domainEvent = notification.DomainEvent;

        await cache.RemoveAsync(CacheKeys.CategoryById(domainEvent.CategoryId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.CategoriesList, cancellationToken);

        if (domainEvent.OldPath is null || domainEvent.NewPath is null || domainEvent.OldPath == domainEvent.NewPath)
            return;

        var categories = await repo.ListAsync(cancellationToken);
        foreach (var category in categories)
        {
            if (category.Id == domainEvent.CategoryId)
                continue;

            if (category.Path.StartsWith(domainEvent.OldPath))
            {
                var suffix = category.Path[domainEvent.OldPath.Length..];
                category.UpdatePath(domainEvent.NewPath + suffix);
            }
        }
    }
}
