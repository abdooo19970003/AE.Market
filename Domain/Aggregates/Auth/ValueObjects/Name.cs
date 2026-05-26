using System.Text.RegularExpressions;

namespace AE.Market.Domain.Aggregates.Auth.ValueObjects
{
    public record Name
    {
        public string Value { get; }
        private static readonly Regex NamesRegex = new(
            @"^[a-zA-Z\p{IsArabic} ]+$",
            RegexOptions.Compiled
        );

        private Name(string value)
        {
            Value = value;
        }

        public static Name Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw Exceptions.Profile.InvalidName;
            if (value.Trim().Length < 3)
                throw Exceptions.Profile.NameTooShort;

            if (!NamesRegex.IsMatch(value))
                throw Exceptions.Profile.InvalidName;

            return new Name(value);
        }
        public static implicit operator Name(string value) => Create (value);
        public static implicit operator string(Name obj) => obj.Value;

    }
}
