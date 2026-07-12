namespace AE.Market.Application.Features.Pricing;

internal static class CacheKeys
{
    internal static string ActivePrice(Guid variantId) => $"price-active-{variantId}";
    internal static string PriceHistory(Guid variantId) => $"price-history-{variantId}";
    internal static string Margin(Guid variantId) => $"price-margin-{variantId}";
}
