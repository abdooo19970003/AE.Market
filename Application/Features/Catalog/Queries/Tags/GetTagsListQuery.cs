using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Tags;

public sealed record GetTagsListQuery(
    int Page = 1,
    int PageSize = 20
) : IBaseQuery<PaginatedList<TagDto>>, ICachedQuery
{
    public string CacheKey => $"tags-list-p{Page}s{PageSize}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
