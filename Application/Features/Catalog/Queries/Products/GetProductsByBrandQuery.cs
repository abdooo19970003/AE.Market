using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;

namespace AE.Market.Application.Features.Catalog.Queries.Products;

public sealed record GetProductsByBrandQuery(
    Guid BrandId,
    int Page = 1,
    int PageSize = 20,
    ProductStatus? Status = null,
    bool? SortDescending = false,
    string? Search="",
    string? SortBy = null
) : QueryFilter(pageNumber: Page, pageSize: PageSize, sortBy: SortBy, sortDescending: SortDescending, search: Search, isActive: true), IBaseQuery<PaginatedList<ProductDto>>, ICachedQuery
{

    public string CacheKey => $"products-brand-{BrandId}-p{Page}s{PageSize}s{Status}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
