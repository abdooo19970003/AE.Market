using AE.Market.Domain.Common;
using AE.Market.Domain.Common.Abstracts;
using System.Text.RegularExpressions;

namespace AE.Market.Domain.Aggregates.Auth.ValueObjects
{
    public record Name : IValueObject
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
                Guard.AgainstNullOrWhiteSpace(value, nameof(Name));
            if (value.Trim().Length < 3)
                Guard.AgainstStringTooShort(value, "AE.Market.Domain.Aggregates.Auth.ValueObjects.Name", 3);
            if (!NamesRegex.IsMatch(value))
                Guard.AgainstInvalidPattern(value, nameof(Name), NamesRegex);

            return new Name(value);
        }
        public static implicit operator Name(string value) => Create (value);
        public static implicit operator string(Name? obj) => obj?.Value ?? string.Empty;

    }
}
