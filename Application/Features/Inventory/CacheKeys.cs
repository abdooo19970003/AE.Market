namespace AE.Market.Application.Features.Inventory;

internal static class CacheKeys
{
    internal static string Stock(Guid variantId) => $"inventory-stock-{variantId}";
    internal static string LowStockReport => "inventory-lowstock-report";
    internal static string AllInventory(int page, int pageSize) => $"inventory-all-{page}-{pageSize}";
}
