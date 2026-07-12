using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;

namespace AE.Market.Application.Features.Catalog.Queries.Products;

public sealed record GetProductsByCategoryQuery(
    Guid CategoryId,
    int Page = 1,
    int PageSize = 20,
    ProductStatus? Status = null,
    string? SortBy = null,
    bool SortDescending = false
) : IBaseQuery<PaginatedList<ProductDto>>, ICachedQuery
{
    public string CacheKey => $"products-category-{CategoryId}-p{Page}s{PageSize}s{Status}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
