using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.ValueObjects;

public sealed class SkuTests
{
    [Theory]
    [InlineData("ABC-123")]
    [InlineData("XI-12P-256-BLK")]
    [InlineData("SKU-001")]
    [InlineData("ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHI")] // 49 chars
    [InlineData("ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ")] // 50 chars
    public void Create_WithValidSku_ReturnsSku(string value)
    {
        var sku = Sku.Create(value);

        sku.Value.Should().Be(value.ToUpperInvariant());
    }

    [Theory]
    [InlineData("XI-12P-256-BLK")]
    [InlineData("abc-def")]
    [InlineData("abc-123")]
    public void Create_UppercasesInput(string value)
    {
        var sku = Sku.Create(value);

        sku.Value.Should().Be(value.ToUpperInvariant());
    }

    [Theory]
    [InlineData("AB")] // too short (2 chars, needs min 3)
    [InlineData("A1")] // too short (2 chars)
    [InlineData("A")]  // too short
    [InlineData("abc def")]
    [InlineData("abc_def")]
    [InlineData("abc.def")]
    [InlineData("abc/def")]
    public void Create_WithInvalidPattern_ThrowsDomainException(string value)
    {
        var act = () => Sku.Create(value);

        act.Should().Throw<DomainException>().WithMessage("*SKU*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyOrWhitespace_ThrowsArgumentNullException(string value)
    {
        var act = () => Sku.Create(value);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNull_ThrowsArgumentNullException()
    {
        var act = () => Sku.Create(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var sku = Sku.Create("ABC-123");

        sku.ToString().Should().Be("ABC-123");
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var sku = Sku.Create("ABC-123");
        string result = sku;

        result.Should().Be("ABC-123");
    }
}
