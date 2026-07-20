namespace AE.Market.Application.Features.Orders;

internal static class CacheKeys
{
    internal static string Order(Guid id) => $"orders-order-{id}";
    internal static string OrderHistory(Guid userId, int page, int pageSize) => $"orders-history-{userId}-p{page}s{pageSize}";
}
