using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.AttributeGroups;

public sealed record GetAttributeGroupsListQuery(
    int Page = 1,
    int PageSize = 20
) : IBaseQuery<PaginatedList<AttributeGroupDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.AttributeGroupsList(Page, PageSize);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
