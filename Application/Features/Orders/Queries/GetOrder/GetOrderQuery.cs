using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Orders.DTOs;

namespace AE.Market.Application.Features.Orders.Queries.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IBaseQuery<OrderDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.Order(OrderId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
