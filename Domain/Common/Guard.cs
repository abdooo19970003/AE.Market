using AE.Market.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace AE.Market.Domain.Common
{
    internal static class Guard
    {
        // Checks if an object or string is null
        public static void AgainstNull(object? input, string parameterName)
        {
            if (input is null)
                throw  new ArgumentNullException(parameterName,
                    $"{parameterName} cannot be null.");
        }

        // Checks if a string is null, empty, or just whitespace
        public static void AgainstNullOrWhiteSpace(string? input, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentNullException(parameterName,
                    $"{parameterName} cannot be null.");
        }

        // Checks if a number falls outside a valid range
        public static void AgainstOutOfRange(int input, string parameterName, int min, int max)
        {
            if (input < min || input > max)
                throw new DomainException(
                    $"OutOfRange.{parameterName}",
                    $"{parameterName} must be between {min} and {max}."
                );
        }

        public static void AgainstInvalidPattern(string input, string parameterName, Regex regex)
        {
            if (!regex.IsMatch(input))
                throw new DomainException($"InvalidPattern.{parameterName}", $" Invalid pattern for {parameterName}");
        }

        public static void AginstInvalidUrl(string input, string parameterName)
        {
            if(!Uri.TryCreate(input, UriKind.Absolute, out _))
                throw new DomainException( $"InvalidPattern.{parameterName}", $" Invalid pattern for {parameterName}");
        }

    }
}
