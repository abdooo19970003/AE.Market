using AE.Market.Domain.Aggregates.Analytics;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Analytics.Specs;

public sealed class SearchAnalyticsByDateRangeSpec : BaseSpecification<SearchAnalytics>
{
    public SearchAnalyticsByDateRangeSpec(DateTime from, DateTime to)
        : base(s => s.SearchedAt >= from && s.SearchedAt <= to)
    {
    }
}
