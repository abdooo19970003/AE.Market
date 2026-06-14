using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Common.Abstracts;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class ShippingDimensionsTests
{
    [Fact]
    public void Create_IsStaticMethod()
    {
        var dimensions = ShippingDimensions.Create(500, 10, 5, 8);

        dimensions.Should().NotBeNull();
        dimensions.WeightInGrams.Should().Be(500);
        dimensions.LongInCentimeter.Should().Be(10);
        dimensions.HeightInCentimeter.Should().Be(5);
        dimensions.WidthInCentimeter.Should().Be(8);
    }

    [Fact]
    public void Create_ImplementsIValueObject()
    {
        var dimensions = ShippingDimensions.Create(500, 10, 5, 8);

        dimensions.Should().BeAssignableTo<IValueObject>();
    }

    [Fact]
    public void Equality_TwoIdenticalRecords_AreEqual()
    {
        var d1 = ShippingDimensions.Create(500, 10, 5, 8);
        var d2 = ShippingDimensions.Create(500, 10, 5, 8);

        d1.Should().Be(d2);
        (d1 == d2).Should().BeTrue();
    }
}
