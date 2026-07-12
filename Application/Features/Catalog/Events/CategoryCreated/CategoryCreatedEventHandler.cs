using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.CategoryCreated;

internal sealed class CategoryCreatedEventHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<CategoryCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CategoryCreatedDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        await cache.RemoveAsync(CacheKeys.CategoriesList, cancellationToken);
    }
}
