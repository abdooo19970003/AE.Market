using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth
{
    public class UserProfile : BaseEntity
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

        private UserProfile(Guid id)
            : base(id) { }

        private UserProfile(Guid id, Guid userId, string firstName, string? lastName)
            : base(id)
        {
            UserId = userId;

            FirstName = firstName;
            if (!string.IsNullOrEmpty(lastName))
                LastName = lastName;
        }

        public static UserProfile Create(
            Guid id,
            Guid userId,
            string firstName,
            string? lastName
        )
        {
            var profile = new UserProfile(id, userId, firstName, lastName);
            profile.AddDominEvent(new UserProfileCreatedEvent(id, userId));
            return profile;
        }

        public UserProfile SetPhoneNumber(string phoneNumber)
        {
            Phone = phoneNumber;
            AddDominEvent(new UserProfileUpdatedEvent(Id));
            return this;
        }

        public UserProfile RemovePhoneNumber()
        {
            Phone = null;
            AddDominEvent(new UserProfileUpdatedEvent(Id));

            return this;
        }

        public UserProfile SetAddress(string city, string country, string? addressline)
        {
            Address = Address.Create(country, city, addressline);
            AddDominEvent(new UserProfileUpdatedEvent(Id));

            return this;
        }

        public UserProfile RemoveAddress()
        {
            Address = null;
            AddDominEvent(new UserProfileUpdatedEvent(Id));

            return this;
        }

        public UserProfile SetProfileImage(string url)
        {
            ProfileImage = url;
            AddDominEvent(new UserProfileUpdatedEvent(Id));

            return this;
        }

        public UserProfile RemoveProfileImage()
        {
            ProfileImage = null;
            AddDominEvent(new UserProfileUpdatedEvent(Id));

            return this;
        }
    }
}
