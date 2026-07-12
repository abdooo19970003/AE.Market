using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Brands;

public sealed record GetBrandsListQuery(
    int Page = 1,
    int PageSize = 20
) : IBaseQuery<PaginatedList<BrandDto>>, ICachedQuery
{
    public string CacheKey => $"brands-list-p{Page}s{PageSize}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
