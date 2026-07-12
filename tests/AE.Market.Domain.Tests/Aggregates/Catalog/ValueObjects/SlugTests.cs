using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.ValueObjects;

public sealed class SlugTests
{
    [Theory]
    [InlineData("hello-world", "hello-world")]
    [InlineData("  hello   world  ", "hello-world")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("abc", "abc")]
    [InlineData("ab", "ab")]
    public void Create_WithValidInput_ReturnsNormalizedSlug(string input, string expected)
    {
        var slug = Slug.Create(input);

        slug.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("hello world!")]
    [InlineData("price: $50")]
    [InlineData("hello@world")]
    [InlineData("a.b.c")]
    public void Create_RemovesIllegalCharacters(string input)
    {
        var slug = Slug.Create(input);

        slug.Value.Should().NotContainAny("!", "@", "$", ".", ":");
    }

    [Fact]
    public void Create_CollapsesMultipleHyphens()
    {
        var slug = Slug.Create("hello---world");

        slug.Value.Should().Be("hello-world");
    }

    [Fact]
    public void Create_TrimsLeadingAndTrailingHyphens()
    {
        var slug = Slug.Create("-hello-world-");

        slug.Value.Should().Be("hello-world");
    }

    [Theory]
    [InlineData("-")]
    [InlineData("--")]
    [InlineData("a")]
    [InlineData("  a  ")]
    public void Create_WithTooShortResult_ThrowsDomainException(string input)
    {
        var act = () => Slug.Create(input);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyOrWhitespace_ThrowsArgumentNullException(string input)
    {
        var act = () => Slug.Create(input);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNull_ThrowsArgumentNullException()
    {
        var act = () => Slug.Create(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void From_DelegatesToCreate()
    {
        var slug = Slug.From("hello-world");

        slug.Value.Should().Be("hello-world");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var slug = Slug.Create("hello-world");

        slug.ToString().Should().Be("hello-world");
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var slug = Slug.Create("hello-world");
        string result = slug;

        result.Should().Be("hello-world");
    }

    [Fact]
    public void CreateNullable_WithNull_ReturnsNull()
    {
        var slug = Slug.CreateNullable(null);

        slug.Should().BeNull();
    }

    [Fact]
    public void CreateNullable_WithValidSlug_ReturnsSlug()
    {
        var slug = Slug.CreateNullable("hello-world");

        slug.Should().NotBeNull();
        slug!.Value.Should().Be("hello-world");
    }
}
