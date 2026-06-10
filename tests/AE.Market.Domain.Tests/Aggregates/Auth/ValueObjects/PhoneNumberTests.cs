using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Auth.ValueObjects;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("05554443322")]
    [InlineData("01234567890")]
    public void Create_WithValidPhoneNumber_ReturnsPhoneNumber(string value)
    {
        var phone = PhoneNumber.Create(value);

        phone.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyString_ThrowsArgumentNullException()
    {
        var act = () => PhoneNumber.Create("");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNull_ThrowsArgumentNullException()
    {
        var act = () => PhoneNumber.Create(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithTooShortNumber_ThrowsDomainException()
    {
        var act = () => PhoneNumber.Create("123");

        act.Should().Throw<DomainException>().Which.Code.Should().Be("Domain.Profile.PhoneNumber.TooShort");
    }

    [Theory]
    [InlineData("0555-444-3322")]
    [InlineData("+201234567890")]
    [InlineData("0555444332a")]
    public void Create_WithInvalidPattern_ThrowsDomainException(string value)
    {
        var act = () => PhoneNumber.Create(value);

        act.Should().Throw<DomainException>().Which.Code.Should().StartWith("InvalidPattern");
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesPhoneNumber()
    {
        PhoneNumber phone = "05554443322";

        phone.Value.Should().Be("05554443322");
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var phone = PhoneNumber.Create("05554443322");
        string result = phone;

        result.Should().Be("05554443322");
    }

    [Fact]
    public void ImplicitConversion_FromNullPhoneNumber_ReturnsEmptyString()
    {
        PhoneNumber? phone = null;
        string result = phone!;

        result.Should().Be(string.Empty);
    }
}
