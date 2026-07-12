namespace AE.Market.Application.Features.Orders;

internal static class CacheKeys
{
    internal static string Order(Guid id) => $"orders-order-{id}";
}
