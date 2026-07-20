using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.BundleItems;

public sealed record GetBundleItemByIdQuery(Guid Id) : IBaseQuery<BundleItemDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.BundleItemById(Id);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
