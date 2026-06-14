using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Exceptions;
using FluentAssertions;
using System.Security.Cryptography;
using System.Text;

namespace AE.Market.Domain.Tests.Aggregates.Auth;

public sealed class UserTests
{
    private static User CreateValidUser()
    {
        return User.Register(
            Guid.NewGuid(),
            "john@example.com",
            "$2a$11$hashedpasswordvalue"
        );
    }

    public sealed class Register
    {
        [Fact]
        public void Register_WithValidData_ReturnsUser()
        {
            var userId = Guid.NewGuid();

            var user = User.Register(userId, "john@example.com", "$2a$11$hashedpassword");

            user.Id.Should().Be(userId);
            user.Email.Value.Should().Be("john@example.com");
            user.PasswordHash.Value.Should().Be("$2a$11$hashedpassword");
        }

        [Fact]
        public void Register_RaisesUserRegisteredDomainEvent()
        {
            var user = CreateValidUser();

            var domainEvent = user.DomainEvents.Should().ContainSingle(e => e is UserRegisteredDomainEvent).Which;
            domainEvent.As<UserRegisteredDomainEvent>().UserId.Should().Be(user.Id);
        }

        [Fact]
        public void Register_WithInvalidEmail_ThrowsException()
        {
            var act = () => User.Register(Guid.NewGuid(), "not-an-email", "$2a$11$hash");

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Register_WithNullEmail_ThrowsException()
        {
            var act = () => User.Register(Guid.NewGuid(), null!, "$2a$11$hash");

            act.Should().Throw<ArgumentNullException>();
        }
    }

    public sealed class Permissions
    {
        [Fact]
        public void AddPermission_AddsPermission()
        {
            var user = CreateValidUser();

            var permission = user.AddPermission(Permission.AccessUsers);

            user.Permissions.Should().ContainSingle()
                .Which.Permission.Should().Be(Permission.AccessUsers);
        }

        [Fact]
        public void RemovePermission_RemovesPermission()
        {
            var user = CreateValidUser();
            var permission = user.AddPermission(Permission.AccessUsers);

            user.RemovePermission(permission);

            user.Permissions.Should().BeEmpty();
        }

        [Fact]
        public void AddPermissions_AddsMultiplePermissions()
        {
            var user = CreateValidUser();
            var permissions = new[] { Permission.AccessUsers, Permission.MutateUsers };

            user.AddPermissions(permissions);

            user.Permissions.Should().HaveCount(2);
        }
    }

    public sealed class RefreshTokens
    {
        [Fact]
        public void AddRefreshToken_AddsToken()
        {
            var user = CreateValidUser();
            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("refresh-token-value")));

            var token = user.AddRefreshToken(hash, TimeSpan.FromDays(10));

            user.RefreshTokens.Should().ContainSingle()
                .Which.TokenHash.Should().Be(hash);
        }

        [Fact]
        public void AddRefreshToken_WhenExceedsLimit_RemovesOldest()
        {
            var user = CreateValidUser();
            var hashes = Enumerable.Range(0, 6)
                .Select(i => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"token-{i}"))))
                .ToArray();

            for (int i = 0; i < 6; i++)
                user.AddRefreshToken(hashes[i], TimeSpan.FromDays(10));

            user.RefreshTokens.Should().HaveCount(5);
            user.RefreshTokens.Should().NotContain(t => t.TokenHash == hashes[0]);
        }

        [Fact]
        public void RevokeRefreshTokens_ClearsAllTokens()
        {
            var user = CreateValidUser();
            var h1 = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("token-1")));
            var h2 = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("token-2")));
            user.AddRefreshToken(h1, TimeSpan.FromDays(10));
            user.AddRefreshToken(h2, TimeSpan.FromDays(10));

            user.RevokeRefreshTokens();

            user.RefreshTokens.Should().BeEmpty();
        }

        [Fact]
        public void RotateRefreshToken_ConsumesOldToken_AddsNewToken()
        {
            var user = CreateValidUser();
            var oldHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("old-token")));
            var newHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("new-token")));
            user.AddRefreshToken(oldHash, TimeSpan.FromDays(10));

            var newToken = user.RotateRefreshToken(oldHash, newHash, TimeSpan.FromDays(10));

            newToken.TokenHash.Should().Be(newHash);
            user.RefreshTokens.Should().Contain(t => t.TokenHash == newHash);
        }

        [Fact]
        public void RotateRefreshToken_WithReusedToken_ThrowsReplayAttack()
        {
            var user = CreateValidUser();
            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("token")));
            var newHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("new-token")));
            user.AddRefreshToken(hash, TimeSpan.FromDays(10));
            user.RotateRefreshToken(hash, newHash, TimeSpan.FromDays(10));

            var act = () => user.RotateRefreshToken(hash, "reused-hash", TimeSpan.FromDays(10));

            act.Should().Throw<DomainException>()
                .Which.Code.Should().Be("Auth.Token.ReplayAttackDetected");
        }

        [Fact]
        public void RotateRefreshToken_WithReusedToken_RevokesAllTokens()
        {
            var user = CreateValidUser();
            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("token")));
            var anotherHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("another-token")));
            var newHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("new-token")));
            user.AddRefreshToken(hash, TimeSpan.FromDays(10));
            user.AddRefreshToken(anotherHash, TimeSpan.FromDays(10));
            user.RotateRefreshToken(hash, newHash, TimeSpan.FromDays(10));

            try { user.RotateRefreshToken(hash, "reused-hash", TimeSpan.FromDays(10)); }
            catch { }

            user.RefreshTokens.Should().BeEmpty();
        }

        [Fact]
        public void RotateRefreshToken_WithReusedToken_RaisesRefreshTokenReusedDomainEvent()
        {
            var user = CreateValidUser();
            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("token")));
            var newHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("new-token")));
            user.AddRefreshToken(hash, TimeSpan.FromDays(10));
            user.RotateRefreshToken(hash, newHash, TimeSpan.FromDays(10));

            try { user.RotateRefreshToken(hash, "reused-hash", TimeSpan.FromDays(10)); }
            catch { }

            user.DomainEvents.Should().Contain(e =>
                e.GetType() == typeof(RefreshTokenReusedDomainEvent)
                && ((RefreshTokenReusedDomainEvent)e).UserId == user.Id
                && ((RefreshTokenReusedDomainEvent)e).UsedToken == hash
            );
        }

        [Fact]
        public void RotateRefreshToken_WithExpiredToken_ThrowsException()
        {
            var user = CreateValidUser();
            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("expired-token")));
            var newHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("new-token")));
            user.AddRefreshToken(hash, TimeSpan.FromDays(-1));

            var act = () => user.RotateRefreshToken(hash, newHash, TimeSpan.FromDays(10));

            act.Should().Throw<DomainException>()
                .Which.Code.Should().Be("Auth.Token.ExpiredOrRevoked");
        }
    }

    public sealed class ProfileManagement
    {
        [Fact]
        public void CreateProfile_AddsProfile()
        {
            var user = CreateValidUser();
            var profileId = Guid.NewGuid();

            user.CreateProfile(profileId, "John", "Doe");

            user.Profile.Should().NotBeNull();
            user.Profile!.Id.Should().Be(profileId);
            user.Profile.FirstName!.Value.Should().Be("John");
        }

        [Fact]
        public void CreateProfile_WhenAlreadyExists_ThrowsInvalidOperationException()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            var act = () => user.CreateProfile(Guid.NewGuid(), "Jane", "Doe");

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CreateProfile_RaisesUserProfileCreatedDomainEvent()
        {
            var user = CreateValidUser();
            var profileId = Guid.NewGuid();

            user.CreateProfile(profileId, "John", "Doe");

            var domainEvent = user.DomainEvents.Should().ContainSingle(e =>
                e.GetType() == typeof(UserProfileCreatedDomainEvent)
            ).Which;
            var created = (UserProfileCreatedDomainEvent)domainEvent;
            created.Id.Should().Be(profileId);
            created.UserId.Should().Be(user.Id);
        }

        [Fact]
        public void UpdateProfileNames_UpdatesNames()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            user.UpdateProfileNames("Jane", "Smith");

            user.Profile!.FirstName!.Value.Should().Be("Jane");
            user.Profile.LastName!.Value.Should().Be("Smith");
        }

        [Fact]
        public void UpdateProfileNames_RaisesUserProfileUpdatedDomainEvent()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            user.UpdateProfileNames("Jane", "Smith");

            user.DomainEvents.Should().Contain(e =>
                e.GetType() == typeof(UserProfileUpdatedDomainEvent)
                && ((UserProfileUpdatedDomainEvent)e).UserId == user.Id
            );
        }

        [Fact]
        public void UpdateProfileNames_WithoutProfile_ThrowsInvalidOperationException()
        {
            var user = CreateValidUser();

            var act = () => user.UpdateProfileNames("Jane", "Smith");

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
