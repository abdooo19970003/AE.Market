using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Products;

public sealed record GetProductsByBrandQuery(
    Guid BrandId,
    int Page = 1,
    int PageSize = 20,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false
) : IBaseQuery<PaginatedList<ProductDto>>, ICachedQuery
{
    public string CacheKey => $"products-brand-{BrandId}-p{Page}s{PageSize}a{IsActive}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
