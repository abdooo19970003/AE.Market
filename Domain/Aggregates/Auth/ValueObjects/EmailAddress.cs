using AE.Market.Domain.Common;
using System.Text.RegularExpressions;

namespace AE.Market.Domain.Aggregates.Auth.ValueObjects
{
    public record EmailAddress : IValueObject
    {
        public string Value { get; }


        private EmailAddress(string value)
        {
            Value = value;
        }

        private static readonly Regex emailPattern = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

        public static EmailAddress Create(string value)
        {
            Guard.AgainstNullOrWhiteSpace(value, "EmailAddress");
            Guard.AgainstInvalidPattern(value, "EmailAddress", emailPattern);

            return  new EmailAddress(value.Trim().ToLower());
        }

        public override string ToString()
        {
            return Value;
        }
        public static implicit operator EmailAddress(string value) => Create(value);
        public static implicit operator string (EmailAddress obj) => obj.Value;

    }
}