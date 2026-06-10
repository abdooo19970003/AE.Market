using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Auth.ValueObjects;

public sealed class AddressTests
{
    [Fact]
    public void Create_WithValidValues_ReturnsAddress()
    {
        var address = Address.Create("Egypt", "Cairo", "Street 1");

        address.Country.Should().Be("Egypt");
        address.City.Should().Be("Cairo");
        address.AddressLine.Should().Be("Street 1");
    }

    [Fact]
    public void Create_WithNullAddressLine_ReturnsAddress()
    {
        var address = Address.Create("Egypt", "Cairo", null);

        address.Country.Should().Be("Egypt");
        address.City.Should().Be("Cairo");
        address.AddressLine.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyCity_ThrowsDomainException()
    {
        var act = () => Address.Create("Egypt", "", "Street 1");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullCountry_ThrowsDomainException()
    {
        var act = () => Address.Create(null!, "Cairo", "Street 1");

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("Egypt,Cairo,Street 1")]
    [InlineData("Egypt,Cairo")]
    [InlineData(" Egypt , Cairo , Street 1 ")]
    public void Parse_WithValidString_ReturnsAddress(string value)
    {
        var address = Address.Parse(value);

        address.Should().NotBeNull();
    }

    [Fact]
    public void Parse_WithSingleValue_ThrowsDomainException()
    {
        var act = () => Address.Parse("Egypt");

        act.Should().Throw<DomainException>().Which.Code.Should().Be("Domain.Profile.Address.ParseFailed");
    }

    [Fact]
    public void Parse_EmptyString_ThrowsDomainException()
    {
        var act = () => Address.Parse("");

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Domain.Profile.Address.ParseFailed");
    }

    [Fact]
    public void ImplicitConversion_FromString_ParsesAddress()
    {
        Address? address = "Egypt,Cairo,Street 1";

        address.Should().NotBeNull();
        address!.Country.Should().Be("Egypt");
    }

    [Fact]
    public void ImplicitConversion_FromInvalidString_ReturnsNull()
    {
        Address? address = "Invalid";

        address.Should().BeNull();
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsCommaSeparated()
    {
        var address = Address.Create("Egypt", "Cairo", "Street 1");
        string result = address;

        result.Should().Be("Egypt,Cairo,Street 1");
    }

    [Fact]
    public void ToString_WithoutAddressLine_OmitsTrailingComma()
    {
        var address = Address.Create("Egypt", "Cairo", null);
        string result = address;

        result.Should().Be("Egypt,Cairo,");
    }
}
