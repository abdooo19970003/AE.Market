using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Enums;
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

    public sealed class DisableEnable
    {
        [Fact]
        public void Disable_SetsIsActiveToFalse()
        {
            var user = CreateValidUser();

            user.Disable();

            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Disable_WhenAlreadyDisabled_DoesNothing()
        {
            var user = CreateValidUser();

            user.Disable();
            user.Disable();

            user.IsActive.Should().BeFalse();
            user.DomainEvents.Should().ContainSingle(e => e is UserDisabledDomainEvent);
        }

        [Fact]
        public void Disable_RaisesUserDisabledDomainEvent()
        {
            var user = CreateValidUser();

            user.Disable();

            user.DomainEvents.Should().Contain(e => e is UserDisabledDomainEvent);
        }

        [Fact]
        public void Enable_SetsIsActiveToTrue()
        {
            var user = CreateValidUser();

            user.Disable();
            user.Enable();

            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Enable_WhenAlreadyEnabled_DoesNothing()
        {
            var user = CreateValidUser();

            user.Enable();

            user.DomainEvents.Should().NotContain(e => e is UserEnabledDomainEvent);
        }

        [Fact]
        public void Enable_RaisesUserEnabledDomainEvent()
        {
            var user = CreateValidUser();

            user.Disable();
            user.Enable();

            user.DomainEvents.Should().Contain(e => e is UserEnabledDomainEvent);
        }
    }

    public sealed class ProfileAddress
    {
        [Fact]
        public void AddProfileAddress_AddsAddress()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            user.AddProfileAddress("Egypt", "Cairo", "Street 1", "Home", true, AddressType.Residence);

            user.Profile!.Addresses.Should().ContainSingle();
            user.Profile!.Addresses.First().City.Should().Be("Cairo");
        }

        [Fact]
        public void AddProfileAddress_WithPrimary_ClearsPreviousPrimary()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");
            user.AddProfileAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);

            user.AddProfileAddress("Egypt", "Alexandria", null, "Office", true, AddressType.Business);

            user.Profile!.Addresses.Count(a => a.IsPrimary).Should().Be(1);
            user.Profile!.Addresses.First(a => a.City == "Alexandria").IsPrimary.Should().BeTrue();
        }

        [Fact]
        public void AddProfileAddress_RaisesUserProfileUpdatedDomainEvent()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            user.AddProfileAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);

            user.DomainEvents.Should().Contain(e => e is UserProfileUpdatedDomainEvent);
        }

        [Fact]
        public void AddProfileAddress_WithoutProfile_ThrowsInvalidOperation()
        {
            var user = CreateValidUser();

            var act = () => user.AddProfileAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void RemoveProfileAddress_RemovesMatchingAddress()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");
            user.AddProfileAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);
            user.AddProfileAddress("Egypt", "Alexandria", null, "Office", false, AddressType.Business);

            var removed = user.RemoveProfileAddress("Egypt", "Alexandria", AddressType.Business);

            removed.Should().BeTrue();
            user.Profile!.Addresses.Should().ContainSingle();
            user.Profile!.Addresses.First().City.Should().Be("Cairo");
        }

        [Fact]
        public void RemoveProfileAddress_WithNoMatch_ReturnsFalse()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");
            user.AddProfileAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);

            var removed = user.RemoveProfileAddress("Egypt", "Giza", AddressType.Residence);

            removed.Should().BeFalse();
            user.Profile!.Addresses.Should().ContainSingle();
        }

        [Fact]
        public void RemoveProfileAddress_WithoutProfile_ThrowsInvalidOperation()
        {
            var user = CreateValidUser();

            var act = () => user.RemoveProfileAddress("Egypt", "Cairo", AddressType.Residence);

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ClearProfileAddresses_EmptiesAddresses()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");
            user.AddProfileAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);
            user.AddProfileAddress("Egypt", "Alexandria", null, "Office", false, AddressType.Business);

            user.ClearProfileAddresses();

            user.Profile!.Addresses.Should().BeEmpty();
        }

        [Fact]
        public void ClearProfileAddresses_WhenEmpty_DoesNotThrow()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            var act = () => user.ClearProfileAddresses();

            act.Should().NotThrow();
        }

        [Fact]
        public void ClearProfileAddresses_RaisesUserProfileUpdatedDomainEvent()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            user.ClearProfileAddresses();

            user.DomainEvents.Should().Contain(e => e is UserProfileUpdatedDomainEvent);
        }
    }

    public sealed class ProfilePhone
    {
        [Fact]
        public void UpdateProfilePhone_SetsPhoneNumber()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            user.UpdateProfilePhone("05554443322");

            user.Profile!.Phone!.Value.Should().Be("05554443322");
        }

        [Fact]
        public void UpdateProfilePhone_WithEmptyString_RemovesPhone()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");
            user.UpdateProfilePhone("05554443322");

            user.UpdateProfilePhone("");

            user.Profile!.Phone.Should().BeNull();
        }

        [Fact]
        public void UpdateProfilePhone_RaisesUserProfileUpdatedDomainEvent()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            user.UpdateProfilePhone("05554443322");

            user.DomainEvents.Should().Contain(e => e is UserProfileUpdatedDomainEvent);
        }

        [Fact]
        public void UpdateProfilePhone_WithoutProfile_ThrowsInvalidOperation()
        {
            var user = CreateValidUser();

            var act = () => user.UpdateProfilePhone("05554443322");

            act.Should().Throw<InvalidOperationException>();
        }
    }

    public sealed class ProfileImage
    {
        [Fact]
        public void UpdateProfileImage_SetsImage()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            user.UpdateProfileImage("https://example.com/image.jpg");

            user.Profile!.ProfileImage!.Value.Should().Be("https://example.com/image.jpg");
        }

        [Fact]
        public void UpdateProfileImage_WithEmptyString_RemovesImage()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");
            user.UpdateProfileImage("https://example.com/image.jpg");

            user.UpdateProfileImage("");

            user.Profile!.ProfileImage.Should().BeNull();
        }

        [Fact]
        public void UpdateProfileImage_RaisesUserProfileUpdatedDomainEvent()
        {
            var user = CreateValidUser();
            user.CreateProfile(Guid.NewGuid(), "John", "Doe");

            user.UpdateProfileImage("https://example.com/image.jpg");

            user.DomainEvents.Should().Contain(e => e is UserProfileUpdatedDomainEvent);
        }

        [Fact]
        public void UpdateProfileImage_WithoutProfile_ThrowsInvalidOperation()
        {
            var user = CreateValidUser();

            var act = () => user.UpdateProfileImage("https://example.com/image.jpg");

            act.Should().Throw<InvalidOperationException>();
        }
    }

    public sealed class RemovePermissions
    {
        [Fact]
        public void RemovePermissions_RemovesMultiple()
        {
            var user = CreateValidUser();
            var p1 = user.AddPermission(Permission.AccessUsers);
            var p2 = user.AddPermission(Permission.MutateUsers);

            user.RemovePermissions(new[] { p1, p2 });

            user.Permissions.Should().BeEmpty();
        }

        [Fact]
        public void RemovePermissions_WithEmptyList_DoesNothing()
        {
            var user = CreateValidUser();
            user.AddPermission(Permission.AccessUsers);

            user.RemovePermissions(Array.Empty<UserPermission>());

            user.Permissions.Should().HaveCount(1);
        }

        [Fact]
        public void RemovePermissions_WithNonExistent_IgnoresSilently()
        {
            var user = CreateValidUser();
            user.AddPermission(Permission.AccessUsers);
            var nonExistent = UserPermission.Create(Guid.NewGuid(), user.Id, Permission.MutateUsers);

            user.RemovePermissions(new[] { nonExistent });

            user.Permissions.Should().HaveCount(1);
        }
    }

    public sealed class DeleteRestore
    {
        [Fact]
        public void Delete_SetsIsDeletedToTrue()
        {
            var user = CreateValidUser();

            user.Delete();

            user.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_WhenAlreadyDeleted_DoesNothing()
        {
            var user = CreateValidUser();

            user.Delete();
            user.Delete();

            user.DomainEvents.Should().ContainSingle(e => e is EntityDeletedDomainEvent);
        }

        [Fact]
        public void Delete_RaisesEntityDeletedDomainEvent()
        {
            var user = CreateValidUser();

            user.Delete();

            user.DomainEvents.Should().Contain(e => e is EntityDeletedDomainEvent);
        }

        [Fact]
        public void Restore_SetsIsDeletedToFalse()
        {
            var user = CreateValidUser();
            user.Delete();

            user.Restore();

            user.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Restore_RaisesEntityRestoredDomainEvent()
        {
            var user = CreateValidUser();
            user.Delete();

            user.Restore();

            user.DomainEvents.Should().Contain(e => e is EntityRestoredDomainEvent);
        }
    }
}
