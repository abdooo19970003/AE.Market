using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Enums;
using AE.Market.Domain.Common.ValueObjects;

namespace AE.Market.Domain.Aggregates.Auth
{
    public sealed class UserProfile : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Name? FirstName { get; private set; }
        public Name? LastName { get; private set; }
        public string FullName =>
            string.Join(" ", new[] { FirstName, LastName }.Where(x => !string.IsNullOrEmpty(x)));
        public PhoneNumber? Phone { get; private set; }
        public ImageUrl? ProfileImage { get; private set; }

        private readonly List<Address> _addresses = [];
        public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

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
            return new UserProfile(id, userId, firstName, lastName);
        }

        internal UserProfile SetPhoneNumber(string phoneNumber)
        {
            Phone = phoneNumber;
            UpdateLastModified();
            return this;
        }

        internal UserProfile RemovePhoneNumber()
        {
            Phone = null;
            UpdateLastModified();

            return this;
        }

        internal Address AddAddress(
            string country,
            string city,
            string? addressLine,
            string? label,
            bool isPrimary,
            AddressType type)
        {
            if (isPrimary)
            {
                for (int i = 0; i < _addresses.Count; i++)
                {
                    if (_addresses[i].IsPrimary)
                        _addresses[i] = _addresses[i].ClearPrimary();
                }
            }

            var address = Address.Create(country, city, addressLine: addressLine, label: label, isPrimary: isPrimary, type: type);
            _addresses.Add(address);
            UpdateLastModified();
            return address;
        }

        internal bool RemoveAddress(string country, string city, AddressType type)
        {
            var address = _addresses.FirstOrDefault(a =>
                a.Country == country && a.City == city && a.Type == type);
            if (address is null) return false;
            _addresses.Remove(address);
            UpdateLastModified();
            return true;
        }

        internal void ClearAddresses()
        {
            _addresses.Clear();
            UpdateLastModified();
        }

        internal UserProfile SetNames(string firstName, string? lastName)
        {
            FirstName = firstName;
            if (!string.IsNullOrEmpty(lastName))
                LastName = lastName;
            UpdateLastModified();

            return this;
        }

        internal UserProfile SetProfileImage(string url)
        {
            ProfileImage = url;
            UpdateLastModified();

            return this;
        }

        internal UserProfile RemoveProfileImage()
        {
            ProfileImage = null;
            UpdateLastModified();

            return this;
        }
    }
}
