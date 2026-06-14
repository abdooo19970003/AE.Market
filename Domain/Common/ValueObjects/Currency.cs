using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Common.ValueObjects;

public sealed record Currency : IValueObject
{
    public string Code { get; }
    public string Symbol { get; }
    public int DecimalPlaces { get; }

    private Currency(string code, string symbol, int decimalPlaces)
    {
        Code = code;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
    }

    private static readonly Dictionary<string, (string Symbol, int Decimals)> Known = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = ("$", 2),
        ["EUR"] = ("€", 2),
        ["GBP"] = ("£", 2),
        ["JPY"] = ("¥", 0),
        ["CAD"] = ("CA$", 2),
        ["AUD"] = ("A$", 2),
        ["CHF"] = ("CHF", 2),
        ["CNY"] = ("¥", 2),
        ["INR"] = ("₹", 2),
        ["BRL"] = ("R$", 2),
        ["KRW"] = ("₩", 0),
        ["MXN"] = ("MX$", 2),
    };

    public static Currency FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Money.UnknownCurrency", "Currency code cannot be null or empty.");

        var normalized = code.Trim().ToUpperInvariant();
        if (!Known.TryGetValue(normalized, out var info))
            throw new DomainException("Money.UnknownCurrency", $"Currency code '{normalized}' is not recognized.");

        return new Currency(normalized, info.Symbol, info.Decimals);
    }

    public static readonly Currency USD = new("USD", "$", 2);
    public static readonly Currency EUR = new("EUR", "€", 2);
    public static readonly Currency GBP = new("GBP", "£", 2);

    public override string ToString() => Code;
}
