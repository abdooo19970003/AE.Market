using AE.Market.Domain.Common.ValueObjects;

namespace AE.Market.Domain.Aggregates.Pricing;

public sealed record FinalPrice
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
    public Money UnitPrice { get; init; } = default!;
    public Money TotalPrice { get; init; } = default!;
    public List<PriceBreakdownItem> Breakdown { get; init; } = [];
}
