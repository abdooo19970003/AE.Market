using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Auth.DTOs;

namespace AE.Market.Application.Features.Auth.Queries.UserById;

public sealed record GetUserByIdQuery(Guid UserId) : IBaseQuery<UserDetailsDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.UserId(UserId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(30);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
