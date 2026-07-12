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
    public string CacheKey => $"products-tag-{TagSlug}-p{Page}s{PageSize}st{Status}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
