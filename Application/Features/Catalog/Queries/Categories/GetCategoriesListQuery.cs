using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Categories;

public sealed record GetCategoriesListQuery(
    int Page = 1,
    int PageSize = 20
) : IBaseQuery<PaginatedList<CategoryDto>>, ICachedQuery
{
    public string CacheKey => $"categories-list-p{Page}s{PageSize}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
