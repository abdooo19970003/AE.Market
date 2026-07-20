using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Auth.Queries.UsersList;

public sealed record GetUsersListQuery(int Page = 1, int PageSize = 10) : IBaseQuery<List<DTOs.UsersListItemDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.UsersList(Page, PageSize);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
