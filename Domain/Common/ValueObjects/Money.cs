using System.Globalization;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.ValueObjects.Errors;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Common.ValueObjects;

public sealed record Money : IValueObject
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, Currency currency)
    {
        return new Money(amount, currency);
    }

    public static Money FromDecimal(decimal amount, string currencyCode)
    {
        return new Money(amount, Currency.FromCode(currencyCode));
    }

    public static Money Zero(Currency currency)
    {
        return new Money(0m, currency);
    }

    public Money Round()
    {
        return new Money(Math.Round(Amount, Currency.DecimalPlaces, MidpointRounding.AwayFromZero), Currency);
    }

    public Money Convert(Currency targetCurrency, decimal exchangeRate)
    {
        if (Currency == targetCurrency)
            return this;
        if (exchangeRate <= 0m)
            throw new DomainException(MoneyErrors.InvalidExchangeRate.Code, MoneyErrors.InvalidExchangeRate.Message);
        return new Money(Amount * exchangeRate, targetCurrency);
    }

    public int CompareTo(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException(MoneyErrors.MismatchedCurrencies.Code,
                $"Cannot compare {Currency.Code} to {other.Currency.Code} directly.");
        return Amount.CompareTo(other.Amount);
    }

    public int CompareTo(Money other, decimal exchangeRateToOther)
    {
        var converted = Convert(other.Currency, exchangeRateToOther);
        return converted.Amount.CompareTo(other.Amount);
    }

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }

    public static Money operator *(decimal multiplier, Money money)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }

    public static Money operator /(Money money, decimal divisor)
    {
        if (divisor == 0m)
            throw new DivideByZeroException("Cannot divide Money by zero.");
        return new Money(money.Amount / divisor, money.Currency);
    }

    public static bool operator <(Money a, Money b) => a.CompareTo(b) < 0;
    public static bool operator >(Money a, Money b) => a.CompareTo(b) > 0;
    public static bool operator <=(Money a, Money b) => a.CompareTo(b) <= 0;
    public static bool operator >=(Money a, Money b) => a.CompareTo(b) >= 0;

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException(MoneyErrors.MismatchedCurrencies.Code,
                $"Cannot perform arithmetic on {a.Currency.Code} and {b.Currency.Code}.");
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:N" + Currency.DecimalPlaces + "} {1}",
            Amount, Currency.Code);
    }
}
