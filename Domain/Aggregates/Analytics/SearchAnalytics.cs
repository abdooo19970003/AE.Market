using AE.Market.Domain.Aggregates.Analytics.Events;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Analytics;

public sealed class SearchAnalytics : BaseEntity, IAggregateRoot
{
    public string SearchText { get; private set; } = default!;
    public string? Filters { get; private set; }
    public int ResultCount { get; private set; }
    public long LatencyMs { get; private set; }
    public string? UserId { get; private set; }
    public DateTime SearchedAt { get; private set; }

    private SearchAnalytics() { }

    public static SearchAnalytics Create(
        string searchText,
        string? filters,
        int resultCount,
        long latencyMs,
        string? userId)
    {
        var entity = new SearchAnalytics
        {
            Id = Guid.NewGuid(),
            SearchText = searchText,
            Filters = filters,
            ResultCount = resultCount,
            LatencyMs = latencyMs,
            UserId = userId,
            SearchedAt = DateTime.UtcNow
        };

        entity.AddDomainEvent(new SearchQueryLoggedDomainEvent(entity.Id));
        return entity;
    }
}
