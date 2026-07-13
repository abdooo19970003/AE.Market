using AE.Market.Domain.Aggregates.Pricing;
using AE.Market.Domain.Common.ValueObjects;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Pricing;

public sealed class MarketplaceTests
{
    [Fact]
    public void Create_WithValidData_ReturnsMarketplace()
    {
        var marketplace = Marketplace.Create(Guid.NewGuid(), "AE-US", "global", Currency.USD);

        marketplace.Code.Should().Be("AE-US");
        marketplace.Region.Should().Be("global");
        marketplace.PreferredCurrency.Should().Be(Currency.USD);
    }

    [Fact]
    public void UpdateDetails_UpdatesCodeAndRegion()
    {
        var marketplace = Marketplace.Create(Guid.NewGuid(), "AE-US", "global", Currency.USD);

        marketplace.UpdateDetails("AE-TR", "turkiye");

        marketplace.Code.Should().Be("AE-TR");
        marketplace.Region.Should().Be("turkiye");
    }
}
