using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.ProductImages;

public sealed record GetProductImagesByProductQuery(Guid ProductId) : IBaseQuery<List<ProductImageDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.ProductImagesByProduct(ProductId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
