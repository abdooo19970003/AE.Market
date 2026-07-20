using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Orders.DTOs;

namespace AE.Market.Application.Features.Orders.Queries.GetOrderHistory;

public sealed record GetOrderHistoryQuery(Guid UserId, int Page = 1, int PageSize = 10) : IBaseQuery<List<OrderDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.OrderHistory(UserId, Page, PageSize);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
