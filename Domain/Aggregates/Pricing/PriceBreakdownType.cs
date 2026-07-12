namespace AE.Market.Domain.Aggregates.Pricing;

public enum PriceBreakdownType
{
    Base = 1,
    Tax = 2,
    Discount = 3,
    Promotion = 4,
    Rounding = 5,
    CurrencyConversion = 6,
}
