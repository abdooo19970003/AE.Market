using AE.Market.Domain.Common.ValueObjects;

namespace AE.Market.Domain.Aggregates.Pricing;

public sealed record PriceBreakdownItem
{
    public string Label { get; init; } = string.Empty;
    public Money Amount { get; init; } = default!;
    public PriceBreakdownType Type { get; init; }
}
