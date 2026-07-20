using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Analytics.Events;

public sealed record SearchQueryLoggedDomainEvent(Guid SearchAnalyticsId) : IDomainEvent;
