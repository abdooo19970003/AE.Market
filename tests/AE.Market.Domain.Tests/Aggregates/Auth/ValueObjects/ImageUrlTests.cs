using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Auth.ValueObjects;

public sealed class ImageUrlTests
{
    [Theory]
    [InlineData("https://example.com/image.jpg")]
    [InlineData("https://cdn.example.com/path/to/image.png")]
    [InlineData("http://example.com/photo.jpeg")]
    public void Create_WithValidUrl_ReturnsImageUrl(string value)
    {
        var url = ImageUrl.Create(value);

        url.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyString_ThrowsDomainException()
    {
        var act = () => ImageUrl.Create("");

        act.Should().Throw<DomainException>().Which.Code.Should().Be("Domain.Profile.ProfileImage.Invalid");
    }

    [Fact]
    public void Create_WithNull_ThrowsDomainException()
    {
        var act = () => ImageUrl.Create(null!);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("Domain.Profile.ProfileImage.Invalid");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("/relative/path")]
    public void Create_WithInvalidUrl_ThrowsDomainException(string value)
    {
        var act = () => ImageUrl.Create(value);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("Domain.Profile.ProfileImage.Invalid");
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesImageUrl()
    {
        ImageUrl url = "https://example.com/image.jpg";

        url.Value.Should().Be("https://example.com/image.jpg");
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var url = ImageUrl.Create("https://example.com/image.jpg");
        string result = url;

        result.Should().Be("https://example.com/image.jpg");
    }

    [Fact]
    public void ImplicitConversion_FromNullImageUrl_ReturnsEmptyString()
    {
        ImageUrl? url = null;
        string result = url!;

        result.Should().Be(string.Empty);
    }
}
