using AE.Market.Domain.Common.Enums;
using AE.Market.Domain.Common.ValueObjects;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Auth.ValueObjects;

public sealed class AddressTests
{
    [Fact]
    public void Create_WithValidValues_ReturnsAddress()
    {
        var address = Address.Create("Egypt", "Cairo", "12345", "Street 1", "Home", true, AddressType.Residence);

        address.Country.Should().Be("Egypt");
        address.City.Should().Be("Cairo");
        address.ZipCode.Should().Be("12345");
        address.AddressLine.Should().Be("Street 1");
        address.Label.Should().Be("Home");
        address.IsPrimary.Should().BeTrue();
        address.Type.Should().Be(AddressType.Residence);
    }

    [Fact]
    public void Create_WithDefaults_ReturnsDefaults()
    {
        var address = Address.Create("Egypt", "Cairo");

        address.IsPrimary.Should().BeFalse();
        address.Type.Should().Be(AddressType.Residence);
        address.Label.Should().BeNull();
        address.ZipCode.Should().BeNull();
        address.AddressLine.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyCity_ThrowsArgumentNullException()
    {
        var act = () => Address.Create("Egypt", "", addressLine: "Street 1");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullCountry_ThrowsArgumentNullException()
    {
        var act = () => Address.Create(null!, "Cairo", addressLine: "Street 1");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithLabel_ReturnsWithNewLabel()
    {
        var address = Address.Create("Egypt", "Cairo");
        var labeled = address.WithLabel("Office");

        labeled.Label.Should().Be("Office");
        address.Label.Should().BeNull();
    }

    [Fact]
    public void MarkPrimary_ReturnsWithIsPrimaryTrue()
    {
        var address = Address.Create("Egypt", "Cairo");
        var primary = address.MarkPrimary();

        primary.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void ClearPrimary_ReturnsWithIsPrimaryFalse()
    {
        var address = Address.Create("Egypt", "Cairo", isPrimary: true);
        var cleared = address.ClearPrimary();

        cleared.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void ChangeType_ReturnsNewType()
    {
        var address = Address.Create("Egypt", "Cairo");
        var billing = address.ChangeType(AddressType.Billing);

        billing.Type.Should().Be(AddressType.Billing);
        address.Type.Should().Be(AddressType.Residence);
    }

    [Fact]
    public void ToString_WithAllFields_ReturnsCommaSeparated()
    {
        var address = Address.Create("Egypt", "Cairo", zipCode: "12345", addressLine: "Street 1");

        address.ToString().Should().Be("Egypt, Cairo, 12345, Street 1");
    }

    [Fact]
    public void ToString_WithoutOptionalFields_OmitsNullParts()
    {
        var address = Address.Create("Egypt", "Cairo");

        address.ToString().Should().Be("Egypt, Cairo");
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = Address.Create("Egypt", "Cairo", "12345", "Street 1", "Home", true, AddressType.Residence);
        var b = Address.Create("Egypt", "Cairo", "12345", "Street 1", "Home", true, AddressType.Residence);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Address.Create("Egypt", "Cairo");
        var b = Address.Create("Egypt", "Alexandria");

        a.Should().NotBe(b);
    }
}
