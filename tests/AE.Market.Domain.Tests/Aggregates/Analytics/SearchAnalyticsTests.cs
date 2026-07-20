using AE.Market.Domain.Aggregates.Analytics;
using AE.Market.Domain.Aggregates.Analytics.Events;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Analytics;

public sealed class SearchAnalyticsTests
{
    [Fact]
    public void Create_WithValidData_ReturnsEntity()
    {
        var entity = SearchAnalytics.Create(
            "laptop",
            """{"categoryId":"abc"}""",
            15,
            42,
            "user-1");

        entity.SearchText.Should().Be("laptop");
        entity.Filters.Should().Be("""{"categoryId":"abc"}""");
        entity.ResultCount.Should().Be(15);
        entity.LatencyMs.Should().Be(42);
        entity.UserId.Should().Be("user-1");
        entity.SearchedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithNullFilters_SetsNull()
    {
        var entity = SearchAnalytics.Create("phone", null, 5, 10, null);

        entity.Filters.Should().BeNull();
        entity.UserId.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesSearchQueryLoggedDomainEvent()
    {
        var entity = SearchAnalytics.Create("test", null, 0, 0, null);

        entity.DomainEvents.Should().Contain(e => e is SearchQueryLoggedDomainEvent);
        var evt = entity.DomainEvents.OfType<SearchQueryLoggedDomainEvent>().Single();
        evt.SearchAnalyticsId.Should().Be(entity.Id);
    }
}
