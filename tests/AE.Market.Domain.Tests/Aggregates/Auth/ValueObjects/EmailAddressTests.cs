using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Auth.ValueObjects;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("john@example.com")]
    [InlineData("john.doe@example.co.uk")]
    public void Create_WithValidEmail_ReturnsEmailAddress(string value)
    {
        var email = EmailAddress.Create(value);

        email.Value.Should().Be(value.Trim().ToLower());
    }

    [Fact]
    public void Create_WithEmptyString_ThrowsArgumentNullException()
    {
        var act = () => EmailAddress.Create("");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNull_ThrowsArgumentNullException()
    {
        var act = () => EmailAddress.Create(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@no-local.com")]
    public void Create_WithInvalidPattern_ThrowsDomainException(string value)
    {
        var act = () => EmailAddress.Create(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_LowercasesEmail()
    {
        var email = EmailAddress.Create("JOHN@EXAMPLE.COM");

        email.Value.Should().Be("john@example.com");
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesEmailAddress()
    {
        EmailAddress email = "john@example.com";

        email.Value.Should().Be("john@example.com");
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var email = EmailAddress.Create("john@example.com");
        string result = email;

        result.Should().Be("john@example.com");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var email = EmailAddress.Create("john@example.com");

        email.ToString().Should().Be("john@example.com");
    }
}
