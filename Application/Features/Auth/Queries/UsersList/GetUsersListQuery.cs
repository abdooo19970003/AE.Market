using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Auth.Queries.UsersList;

public sealed record GetUsersListQuery : IBaseQuery<List<DTOs.UsersListItemDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.UsersList(1, 1);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
