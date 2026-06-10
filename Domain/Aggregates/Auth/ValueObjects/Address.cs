using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.ValueObjects
{
    public  record Address : IValueObject
    {
        private Address(string country, string city, string? addressLine)
        {
            AddressLine = addressLine;
            City = city;
            Country = country;
        }

        public string? AddressLine { get; init; }
        public string? City { get; init; }
        public string? Country { get; init; }

        public static Address Create(string country, string city, string? addressLine)
        {
            Guard.AgainstNullOrWhiteSpace(city, "Address.City");
            Guard.AgainstNullOrWhiteSpace(country, "Address.Country");
            return new Address(country, city, addressLine);
        }

        public static Address Parse(string value)
        {
            var strings = value.Split(',');
            if (strings.Length < 2)
                throw Exceptions.Profile.FailedParseAddress;
            var address = Create(strings[0], strings[1], string.Join(",", strings.Skip(2)));
            return address;
        }

        public static implicit operator Address?(string value)
        {
            try { 
            return Parse(value);
            }
            catch { return null; }
        }

        public static implicit operator string(Address value) =>
            string.Join(",", [value.Country, value.City, value.AddressLine]);

    }
}
