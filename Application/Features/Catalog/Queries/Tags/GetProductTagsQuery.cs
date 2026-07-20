using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Tags;

public sealed record GetProductTagsQuery(Guid ProductId) : IBaseQuery<List<TagDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.ProductTags(ProductId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
