using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Features.Auth.Queries.Me;

public sealed record GetMeQuery(Guid UserId) : IBaseQuery<UserDetailsDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.UserId(UserId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
