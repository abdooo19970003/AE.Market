using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Pricing.Errors;

public static class MarketplaceErrors
{
    public static readonly Error MarketplaceNotFound = new(
        "Pricing.Marketplace.NotFound",
        "The marketplace was not found.");

    public static readonly Error DuplicateCode = new(
        "Pricing.Marketplace.DuplicateCode",
        "A marketplace with this code already exists.");
}
