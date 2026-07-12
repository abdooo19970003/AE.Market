using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Inventory.DTOs;

namespace AE.Market.Application.Features.Inventory.Queries.GetStock;

public sealed record GetStockQuery(
    Guid VariantId
) : IBaseQuery<StockDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.Stock(VariantId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
