using AE.Market.Domain.Common;
using AE.Market.Domain.Common.Abstracts;
using System.Text.RegularExpressions;

namespace AE.Market.Domain.Aggregates.Auth.ValueObjects
{
    public record PhoneNumber : IValueObject
    {
        public string Value { get; }
       

        private PhoneNumber(string value)
        {
            Value = value;
        }
        private static readonly Regex PhoneRegex = new(@"^\d{11}$", RegexOptions.Compiled);

        public static PhoneNumber Create(string value)
        {
            Guard.AgainstNullOrWhiteSpace(value,nameof(PhoneNumber));
            if (value.Trim().Length < 7)
                throw Exceptions.Profile.PhoneNumberTooShort;

            Guard.AgainstInvalidPattern(value, "PhoneNumber", PhoneRegex);
            return new PhoneNumber(value);
        }
        public static implicit operator PhoneNumber(string value) => Create(value);
        public static implicit operator string(PhoneNumber? obj) => obj?.Value ?? string.Empty;

    }
}
