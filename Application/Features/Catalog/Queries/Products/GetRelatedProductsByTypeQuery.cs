using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;

namespace AE.Market.Application.Features.Catalog.Queries.Products;

public sealed record GetRelatedProductsByTypeQuery(
    Guid ProductId,
    RelationType Type,
    int Page = 1,
    int PageSize = 20,
    ProductStatus? Status = null
) : IBaseQuery<PaginatedList<ProductDto>>, ICachedQuery
{
    public string CacheKey => $"related-products-{ProductId}-{Type}-p{Page}s{PageSize}st{Status}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(10);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
