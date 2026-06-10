using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Auth.ValueObjects;

public sealed class PasswordHashTests
{
    [Fact]
    public void FromHashedString_WithValidValue_ReturnsPasswordHash()
    {
        var hash = PasswordHash.FromHashedString("$2a$11$hashedvalue123");

        hash.Value.Should().Be("$2a$11$hashedvalue123");
    }

    [Fact]
    public void FromHashedString_WithEmptyString_ThrowsArgumentNullException()
    {
        var act = () => PasswordHash.FromHashedString("");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromHashedString_WithNull_ThrowsArgumentNullException()
    {
        var act = () => PasswordHash.FromHashedString(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesPasswordHash()
    {
        PasswordHash hash = "$2a$11$hashedvalue123";

        hash.Value.Should().Be("$2a$11$hashedvalue123");
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var hash = PasswordHash.FromHashedString("$2a$11$hashedvalue123");
        string result = hash;

        result.Should().Be("$2a$11$hashedvalue123");
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var hash1 = PasswordHash.FromHashedString("$2a$11$hash");
        var hash2 = PasswordHash.FromHashedString("$2a$11$hash");

        hash1.Should().Be(hash2);
    }
}
