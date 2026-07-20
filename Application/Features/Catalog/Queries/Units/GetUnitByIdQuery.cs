using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Units;

public sealed record GetUnitByIdQuery(Guid Id) : IBaseQuery<UnitDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.UnitById(Id);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
