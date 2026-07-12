namespace AE.Market.Domain.Aggregates.Prices;

public enum PriceChangeReason
{
    Initial = 1,
    ManualUpdate = 2,
    Promotion = 3,
    CostAdjustment = 4,
    TierChange = 5
}
