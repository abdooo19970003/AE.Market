using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.ProductImages;

public sealed record GetProductImageByIdQuery(Guid Id) : IBaseQuery<ProductImageDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.ProductImageById(Id);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
