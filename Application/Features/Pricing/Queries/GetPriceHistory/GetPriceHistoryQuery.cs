using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Pricing.DTOs;

namespace AE.Market.Application.Features.Pricing.Queries.GetPriceHistory;

public sealed record GetPriceHistoryQuery(
    Guid VariantId,
    int Page = 1,
    int PageSize = 20
) : IBaseQuery<List<PriceHistoryDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.PriceHistory(VariantId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
