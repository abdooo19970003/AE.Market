using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events.RequiredAttributeAddedToCategory;

internal sealed class RequiredAttributeAddedToCategoryEventHandler
    : INotificationHandler<DomainEventNotification<RequiredAttributeAddedToCategoryDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<RequiredAttributeAddedToCategoryDomainEvent> notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
