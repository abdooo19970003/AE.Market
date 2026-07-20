using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;

namespace AE.Market.Application.Features.Catalog.Queries.Products;

public sealed record GetProductsByTagQuery(
    string TagSlug,
    int Page = 1,
    int PageSize = 20,
    ProductStatus? Status = null
) : IBaseQuery<PaginatedList<ProductDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.ProductsByTag(TagSlug, Page, PageSize);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
