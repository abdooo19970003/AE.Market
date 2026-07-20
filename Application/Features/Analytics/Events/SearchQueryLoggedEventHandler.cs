using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.Analytics.Events;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Events;

// No-op handler: SearchAnalytics is already persisted by RecordSearchQueryHandler.
// This handler exists so the domain event is acknowledged by the outbox pipeline
// and doesn't cause "unhandled notification" warnings.
internal sealed class SearchQueryLoggedEventHandler
    : INotificationHandler<DomainEventNotification<SearchQueryLoggedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<SearchQueryLoggedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
