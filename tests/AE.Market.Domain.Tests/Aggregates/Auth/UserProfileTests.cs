using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Common.Enums;
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
    }

    public sealed class Address
    {
        [Fact]
        public void AddAddress_AddsToCollection()
        {
            var profile = CreateProfile();

            profile.AddAddress("Egypt", "Cairo", "Street 1", "Home", true, AddressType.Residence);

            profile.Addresses.Should().ContainSingle();
            profile.Addresses.First().City.Should().Be("Cairo");
            profile.Addresses.First().Country.Should().Be("Egypt");
            profile.Addresses.First().Label.Should().Be("Home");
            profile.Addresses.First().IsPrimary.Should().BeTrue();
            profile.Addresses.First().Type.Should().Be(AddressType.Residence);
        }

        [Fact]
        public void AddAddress_MultipleAddresses_AreAdded()
        {
            var profile = CreateProfile();

            profile.AddAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);
            profile.AddAddress("Egypt", "Alexandria", null, "Office", false, AddressType.Business);

            profile.Addresses.Should().HaveCount(2);
        }

        [Fact]
        public void AddAddress_WithPrimary_ClearsPreviousPrimary()
        {
            var profile = CreateProfile();
            profile.AddAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);

            profile.AddAddress("Egypt", "Alexandria", null, "Office", true, AddressType.Business);

            profile.Addresses.Count(a => a.IsPrimary).Should().Be(1);
            profile.Addresses.First(a => a.City == "Alexandria").IsPrimary.Should().BeTrue();
        }

        [Fact]
        public void RemoveAddress_RemovesMatchingAddress()
        {
            var profile = CreateProfile();
            profile.AddAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);
            profile.AddAddress("Egypt", "Alexandria", null, "Office", false, AddressType.Business);

            var removed = profile.RemoveAddress("Egypt", "Alexandria", AddressType.Business);

            removed.Should().BeTrue();
            profile.Addresses.Should().ContainSingle();
            profile.Addresses.First().City.Should().Be("Cairo");
        }

        [Fact]
        public void RemoveAddress_NonExistent_ReturnsFalse()
        {
            var profile = CreateProfile();
            profile.AddAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);

            var removed = profile.RemoveAddress("Egypt", "Giza", AddressType.Residence);

            removed.Should().BeFalse();
            profile.Addresses.Should().ContainSingle();
        }

        [Fact]
        public void ClearAddresses_EmptiesCollection()
        {
            var profile = CreateProfile();
            profile.AddAddress("Egypt", "Cairo", null, "Home", true, AddressType.Residence);
            profile.AddAddress("Egypt", "Alexandria", null, "Office", false, AddressType.Business);

            profile.ClearAddresses();

            profile.Addresses.Should().BeEmpty();
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
    }
}
