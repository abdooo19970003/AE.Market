using AE.Market.Domain.Aggregates.Pricing;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Pricing;

public sealed class MarketplaceTaxRateTests
{
    [Fact]
    public void Create_WithValidData_ReturnsTaxRate()
    {
        var taxRate = MarketplaceTaxRate.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.20m);

        taxRate.TaxRate.Should().Be(0.20m);
        taxRate.IsActive().Should().BeTrue();
    }

    [Fact]
    public void Deactivate_SetsValidTo()
    {
        var taxRate = MarketplaceTaxRate.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.20m);

        taxRate.Deactivate();

        taxRate.ValidTo.Should().NotBeNull();
        taxRate.IsActive().Should().BeFalse();
    }
}
