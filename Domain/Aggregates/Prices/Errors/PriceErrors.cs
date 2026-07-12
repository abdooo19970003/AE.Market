using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Prices.Errors;

public static class PriceErrors
{
    public static readonly Error PriceNotFound = new(
        "Prices.Price.NotFound",
        "The specified price was not found."
    );

    public static readonly Error InvalidDateRange = new(
        "Prices.Price.InvalidDateRange",
        "ValidTo must be after ValidFrom."
    );

    public static readonly Error DuplicateActiveSalePrice = new(
        "Prices.Price.DuplicateActiveSalePrice",
        "An active sale price already exists for this variant."
    );

    public static readonly Error PriceMustBePositive = new(
        "Prices.Price.PriceMustBePositive",
        "Price amount must be greater than zero."
    );

    public static readonly Error NoActivePrice = new(
        "Prices.Price.NoActivePrice",
        "No active price found for this variant."
    );

    public static readonly Error PriceAlreadyInactive = new(
        "Prices.Price.AlreadyInactive",
        "The price is already inactive."
    );
}
