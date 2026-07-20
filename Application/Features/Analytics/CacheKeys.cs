namespace AE.Market.Application.Features.Analytics;

internal static class CacheKeys
{
    internal static string AdminStats() => "analytics-admin-stats";
    internal static string TopProducts(int days, int top) => $"analytics-top-products-{days}-{top}";
    internal static string TopSearches(int days, int top) => $"analytics-top-searches-{days}-{top}";
}
