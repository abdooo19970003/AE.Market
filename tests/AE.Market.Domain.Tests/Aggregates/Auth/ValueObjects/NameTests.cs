using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Auth.ValueObjects;

public sealed class NameTests
{
    [Theory]
    [InlineData("John")]
    [InlineData("أحمد")]
    [InlineData("John Michael")]
    public void Create_WithValidName_ReturnsName(string value)
    {
        var name = Name.Create(value);

        name.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyString_ThrowsArgumentNullException()
    {
        var act = () => Name.Create("");

        act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be(nameof(Name));
    }

    [Fact]
    public void Create_WithWhiteSpace_ThrowsArgumentNullException()
    {
        var act = () => Name.Create("   ");

        act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be(nameof(Name));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Create_WithTooShortName_ThrowsDomainException(string value)
    {
        var act = () => Name.Create(value);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("AE.Market.Domain.Aggregates.Auth.ValueObjects.Name");
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("John!@#")]
    [InlineData("John_Doe")]
    public void Create_WithInvalidCharacters_ThrowsDomainException(string value)
    {
        var act = () => Name.Create(value);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("InvalidPattern.Name");
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesName()
    {
        Name name = "John";

        name.Value.Should().Be("John");
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var name = Name.Create("John");
        string result = name;

        result.Should().Be("John");
    }

    [Fact]
    public void ImplicitConversion_FromNullName_ReturnsEmptyString()
    {
        Name? name = null;
        string result = name!;

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var name1 = Name.Create("John");
        var name2 = Name.Create("John");

        name1.Should().Be(name2);
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHash()
    {
        var name1 = Name.Create("John");
        var name2 = Name.Create("John");

        name1.GetHashCode().Should().Be(name2.GetHashCode());
    }
}
