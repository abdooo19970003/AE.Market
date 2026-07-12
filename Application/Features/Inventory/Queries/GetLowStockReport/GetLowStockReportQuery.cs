using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Inventory.DTOs;

namespace AE.Market.Application.Features.Inventory.Queries.GetLowStockReport;

public sealed record GetLowStockReportQuery(
    int Page = 1,
    int PageSize = 20
) : IBaseQuery<List<LowStockReportDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.LowStockReport;
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(2);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
