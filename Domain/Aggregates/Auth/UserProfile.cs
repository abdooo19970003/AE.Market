using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth
{
    public sealed class UserProfile : BaseEntity
    {
        public Guid UserId { get; private set; }
        public User? User { get; private set; } = null;

        public Name? FirstName { get; private set; }
        public Name? LastName { get; private set; }
        public string FullName =>
            string.Join(" ", new[] { FirstName, LastName }.Where(x => !string.IsNullOrEmpty(x)));
        public PhoneNumber? Phone { get; private set; }
        public Address? Address { get; private set; }
        public ImageUrl? ProfileImage { get; private set; }

        private UserProfile() { }

        private UserProfile(Guid id, Guid userId, string firstName, string? lastName)
            : base(id)
        {
            UserId = userId;

            FirstName = firstName;
            if (!string.IsNullOrEmpty(lastName))
                LastName = lastName;
        }

        internal static UserProfile Create(Guid id, Guid userId, string firstName, string? lastName)
        {
            var profile = new UserProfile(id, userId, firstName, lastName);
            profile.AddDomainEvent(new UserProfileCreatedDomainEvent(id, userId));
            return profile;
        }

        internal UserProfile SetPhoneNumber(string phoneNumber)
        {
            Phone = phoneNumber;
            AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));
            return this;
        }

        internal UserProfile RemovePhoneNumber()
        {
            Phone = null;
            AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));

            return this;
        }

        internal UserProfile SetAddress(string city, string country, string? addressline)
        {
            Address = Address.Create(country, city, addressline);
            AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));

            return this;
        }

        internal UserProfile RemoveAddress()
        {
            Address = null;
            AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));

            return this;
        }

        internal UserProfile SetProfileImage(string url)
        {
            ProfileImage = url;
            AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));

            return this;
        }

        internal UserProfile RemoveProfileImage()
        {
            ProfileImage = null;
            AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));

            return this;
        }
    }
}
