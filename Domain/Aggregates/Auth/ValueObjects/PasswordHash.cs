using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.ValueObjects
{
    public record PasswordHash : IValueObject
    {
        public string Value { get; }

        private PasswordHash(string value)
        {
            Value = value;
        }

        public static PasswordHash FromHashedString(string hashedValue)
        {
            Guard.AgainstNullOrWhiteSpace(hashedValue, nameof(PasswordHash));
            return new PasswordHash(hashedValue);
        }

        public static implicit operator PasswordHash(string hashedValue) =>
            FromHashedString(hashedValue);

        public static implicit operator string(PasswordHash hashedValue) => hashedValue.Value;
    }
}
