using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Inventory.DTOs;

namespace AE.Market.Application.Features.Inventory.Queries.GetAllInventory;

public sealed record GetAllInventoryQuery(
    int Page = 1,
    int PageSize = 20
) : IBaseQuery<List<InventoryItemDto>>, ICachedQuery
{
    public string CacheKey => $"inventory-all-{Page}-{PageSize}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
