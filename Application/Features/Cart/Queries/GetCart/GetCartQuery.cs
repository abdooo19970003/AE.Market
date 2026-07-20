using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Cart.DTOs;

namespace AE.Market.Application.Features.Cart.Queries.GetCart;

public sealed record GetCartQuery(
    Guid? UserId,
    Guid? SessionId
) : IBaseQuery<CartDto>, ICachedQuery
{
    public string CacheKey => UserId.HasValue
        ? CacheKeys.CartByUser(UserId.Value)
        : CacheKeys.CartBySession(SessionId!.Value);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(2);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
