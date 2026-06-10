using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Auth;

public sealed class UserProfileTests
{
    private static UserProfile CreateProfile()
    {
        return UserProfile.Create(Guid.NewGuid(), Guid.NewGuid(), "John", "Doe");
    }

    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_ReturnsProfile()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var profile = UserProfile.Create(id, userId, "John", "Doe");

            profile.Id.Should().Be(id);
            profile.UserId.Should().Be(userId);
            profile.FirstName!.Value.Should().Be("John");
            profile.LastName!.Value.Should().Be("Doe");
        }

        [Fact]
        public void Create_WithoutLastName_ReturnsProfile()
        {
            var profile = UserProfile.Create(Guid.NewGuid(), Guid.NewGuid(), "John", null);

            profile.LastName.Should().BeNull();
        }

        [Fact]
        public void Create_RaisesUserProfileCreatedDomainEvent()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var profile = UserProfile.Create(id, userId, "John", "Doe");

            profile.DomainEvents.Should().ContainSingle(e =>
                e.GetType() == typeof(UserProfileCreatedDomainEvent)
                && ((UserProfileCreatedDomainEvent)e).Id == id
                && ((UserProfileCreatedDomainEvent)e).UserId == userId
            );
        }

        [Fact]
        public void FullName_WithFirstAndLastName_ReturnsCombined()
        {
            var profile = UserProfile.Create(Guid.NewGuid(), Guid.NewGuid(), "John", "Doe");

            profile.FullName.Should().Be("Name { Value = John } Name { Value = Doe }");
        }

        [Fact]
        public void FullName_WithoutLastName_ReturnsOnlyFirstName()
        {
            var profile = UserProfile.Create(Guid.NewGuid(), Guid.NewGuid(), "John", null);

            profile.FullName.Should().Be("Name { Value = John }");
        }
    }

    public sealed class SetNames
    {
        [Fact]
        public void SetNames_WithValidData_UpdatesNames()
        {
            var profile = CreateProfile();

            profile.SetNames("Jane", "Smith");

            profile.FirstName!.Value.Should().Be("Jane");
            profile.LastName!.Value.Should().Be("Smith");
        }


        [Fact]
        public void SetNames_RaisesUserProfileUpdatedDomainEvent()
        {
            var profile = CreateProfile();

            profile.SetNames("Jane", "Smith");

            profile.DomainEvents.Should().Contain(e =>
                e.GetType() == typeof(UserProfileUpdatedDomainEvent)
                && ((UserProfileUpdatedDomainEvent)e).UserId == profile.UserId
            );
        }

        [Fact]
        public void SetNames_ReturnsSelfForChaining()
        {
            var profile = CreateProfile();

            var result = profile.SetNames("Jane", "Smith");

            result.Should().BeSameAs(profile);
        }
    }

    public sealed class PhoneNumber
    {
        [Fact]
        public void SetPhoneNumber_SetsPhone()
        {
            var profile = CreateProfile();

            profile.SetPhoneNumber("05554443322");

            profile.Phone!.Value.Should().Be("05554443322");
        }

        [Fact]
        public void RemovePhoneNumber_ClearsPhone()
        {
            var profile = CreateProfile();
            profile.SetPhoneNumber("05554443322");

            profile.RemovePhoneNumber();

            profile.Phone.Should().BeNull();
        }

        [Fact]
        public void SetPhoneNumber_RaisesUserProfileUpdatedDomainEvent()
        {
            var profile = CreateProfile();

            profile.SetPhoneNumber("05554443322");

            profile.DomainEvents.Should().Contain(e =>
                e.GetType() == typeof(UserProfileUpdatedDomainEvent)
                && ((UserProfileUpdatedDomainEvent)e).UserId == profile.UserId
            );
        }
    }

    public sealed class Address
    {
        [Fact]
        public void SetAddress_SetsAddress()
        {
            var profile = CreateProfile();

            profile.SetAddress("Cairo", "Egypt", "Street 1");

            profile.Address.Should().NotBeNull();
            profile.Address!.City.Should().Be("Cairo");
            profile.Address.Country.Should().Be("Egypt");
        }

        [Fact]
        public void RemoveAddress_ClearsAddress()
        {
            var profile = CreateProfile();
            profile.SetAddress("Cairo", "Egypt", "Street 1");

            profile.RemoveAddress();

            profile.Address.Should().BeNull();
        }

        [Fact]
        public void SetAddress_RaisesUserProfileUpdatedDomainEvent()
        {
            var profile = CreateProfile();

            profile.SetAddress("Cairo", "Egypt", "Street 1");

            profile.DomainEvents.Should().Contain(e =>
                e.GetType() == typeof(UserProfileUpdatedDomainEvent)
                && ((UserProfileUpdatedDomainEvent)e).UserId == profile.UserId
            );
        }
    }

    public sealed class ProfileImage
    {
        [Fact]
        public void SetProfileImage_SetsImage()
        {
            var profile = CreateProfile();

            profile.SetProfileImage("https://example.com/image.jpg");

            profile.ProfileImage!.Value.Should().Be("https://example.com/image.jpg");
        }

        [Fact]
        public void RemoveProfileImage_ClearsImage()
        {
            var profile = CreateProfile();
            profile.SetProfileImage("https://example.com/image.jpg");

            profile.RemoveProfileImage();

            profile.ProfileImage.Should().BeNull();
        }

        [Fact]
        public void SetProfileImage_RaisesUserProfileUpdatedDomainEvent()
        {
            var profile = CreateProfile();

            profile.SetProfileImage("https://example.com/image.jpg");

            profile.DomainEvents.Should().Contain(e =>
                e.GetType() == typeof(UserProfileUpdatedDomainEvent)
                && ((UserProfileUpdatedDomainEvent)e).UserId == profile.UserId
            );
        }
    }
}
