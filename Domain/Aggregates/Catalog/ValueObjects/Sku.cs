using AE.Market.Domain.Common;
using AE.Market.Domain.Common.Abstracts;
using System.Text.RegularExpressions;

namespace AE.Market.Domain.Aggregates.Catalog.ValueObjects;

public sealed record Sku : IValueObject
{
    public string Value { get; }

    private static readonly Regex SkuPattern = new(
        @"^[A-Z0-9][A-Z0-9\-]{2,49}$",
        RegexOptions.Compiled
    );

    private Sku(string value) => Value = value;

    public static Sku Create(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(Sku));
        var sku = value.Trim().ToUpperInvariant();

        if (!SkuPattern.IsMatch(sku))
            throw new Exceptions.DomainException(
                "Catalog.Sku.InvalidFormat",
                "SKU must be 3-50 characters, uppercase letters, digits, and hyphens only."
            );

        return new Sku(sku);
    }

    public override string ToString() => Value;

    public static implicit operator string(Sku sku) => sku.Value;
}
