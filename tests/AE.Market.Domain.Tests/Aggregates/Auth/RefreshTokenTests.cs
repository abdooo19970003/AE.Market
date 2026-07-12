using AE.Market.Domain.Aggregates.Auth;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Auth;

public sealed class RefreshTokenTests
{
    private static RefreshToken CreateValidToken()
    {
        return RefreshToken.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "hashed-token-value",
            DateTime.UtcNow.AddDays(10)
        );
    }

    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_SetsProperties()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var hash = "hashed-token-value";
            var expiry = DateTime.UtcNow.AddDays(10);

            var token = RefreshToken.Create(id, userId, hash, expiry);

            token.Id.Should().Be(id);
            token.UserId.Should().Be(userId);
            token.TokenHash.Should().Be(hash);
            token.ExpiryTime.Should().Be(expiry);
        }
    }

    public sealed class MarkConsumed
    {
        [Fact]
        public void MarkConsumed_SetsConsumedAt()
        {
            var token = CreateValidToken();

            token.MarkConsumed();

            token.ConsumedAt.Should().NotBeNull();
        }
    }

    public sealed class IsExpired
    {
        [Fact]
        public void IsExpired_WhenExpiryPassed_ReturnsTrue()
        {
            var token = RefreshToken.Create(
                Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(-1));

            token.IsExpired.Should().BeTrue();
        }

        [Fact]
        public void IsExpired_WhenNotExpired_ReturnsFalse()
        {
            var token = CreateValidToken();

            token.IsExpired.Should().BeFalse();
        }
    }
}
