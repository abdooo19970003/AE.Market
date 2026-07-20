using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Orders.DTOs;

namespace AE.Market.Application.Features.Orders.Queries.GetOrderHistory;

public sealed record GetOrderHistoryQuery(Guid UserId) : IBaseQuery<List<OrderDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.OrderHistory(UserId, 1, 1);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
