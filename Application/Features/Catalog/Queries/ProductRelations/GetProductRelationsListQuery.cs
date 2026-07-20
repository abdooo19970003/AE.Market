using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.ProductRelations;

public sealed record GetProductRelationsListQuery(
    int Page = 1,
    int PageSize = 20
) : IBaseQuery<PaginatedList<ProductRelationDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.ProductRelationsList(Page, PageSize);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
