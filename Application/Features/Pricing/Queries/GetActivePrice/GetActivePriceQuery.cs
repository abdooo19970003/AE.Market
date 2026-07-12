using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Pricing.DTOs;

namespace AE.Market.Application.Features.Pricing.Queries.GetActivePrice;

public sealed record GetActivePriceQuery(
    Guid VariantId
) : IBaseQuery<PriceDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.ActivePrice(VariantId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(10);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
