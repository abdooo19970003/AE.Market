using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Pricing.DTOs;

namespace AE.Market.Application.Features.Pricing.Queries.GetMargin;

public sealed record GetMarginQuery(
    Guid VariantId
) : IBaseQuery<MarginDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.Margin(VariantId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(10);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
