using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.ValueObjects;

public sealed class URLTests
{
    [Fact]
    public void Create_WithTwoSegments_ReturnsJoinedUrl()
    {
        var url = URL.Create("electronics", "phones");

        url.Value.Should().Be("/electronics/phones");
    }

    [Fact]
    public void Create_WithSingleSegment_ReturnsUrl()
    {
        var url = URL.Create("electronics");

        url.Value.Should().Be("/electronics");
    }

    [Fact]
    public void Create_WithThreeSegments_ReturnsJoinedUrl()
    {
        var url = URL.Create("electronics", "phones", "smartphones");

        url.Value.Should().Be("/electronics/phones/smartphones");
    }

    [Fact]
    public void Create_TrimsSlashesFromSegments()
    {
        var url = URL.Create("/electronics/", "/phones/");

        url.Value.Should().Be("/electronics/phones");
    }

    [Fact]
    public void Create_FiltersOutNullAndWhitespaceSegments()
    {
        var url = URL.Create("electronics", null!, "", "phones");

        url.Value.Should().Be("/electronics/phones");
    }

    [Fact]
    public void Create_WithNoSegments_ThrowsArgumentException()
    {
        var act = () => URL.Create();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithAllInvalidSegments_ThrowsArgumentException()
    {
        var act = () => URL.Create(null!, "", " ");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("path?query=1")]
    [InlineData("path#fragment")]
    [InlineData("path&more")]
    public void Create_WithIllegalCharacters_ThrowsArgumentException(string segment)
    {
        var act = () => URL.Create("valid", segment);

        act.Should().Throw<ArgumentException>();
    }
}
