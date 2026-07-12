using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Products;

public sealed record GetProductByIdQuery(Guid Id, bool IncludeChildren = false)
    : IBaseQuery<ProductDetailDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.ProductById(Id);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(30);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
