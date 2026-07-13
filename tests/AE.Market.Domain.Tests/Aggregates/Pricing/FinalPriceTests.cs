using AE.Market.Domain.Aggregates.Pricing;
using AE.Market.Domain.Common.ValueObjects;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Pricing;

public sealed class FinalPriceTests
{
    [Fact]
    public void FinalPrice_RecordEquality_Works()
    {
        var price1 = new FinalPrice
        {
            VariantId = Guid.NewGuid(),
            Quantity = 2,
            UnitPrice = Money.Create(10m, Currency.USD),
            TotalPrice = Money.Create(20m, Currency.USD),
        };
        var price2 = price1 with { };

        price1.Should().Be(price2);
    }

    [Fact]
    public void BreakdownItem_CanBeConstructed()
    {
        var item = new PriceBreakdownItem
        {
            Label = "Tax (20%)",
            Amount = Money.Create(2m, Currency.USD),
            Type = PriceBreakdownType.Tax,
        };

        item.Label.Should().Be("Tax (20%)");
        item.Type.Should().Be(PriceBreakdownType.Tax);
    }
}
