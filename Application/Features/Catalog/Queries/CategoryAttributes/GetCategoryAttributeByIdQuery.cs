using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.CategoryAttributes;

public sealed record GetCategoryAttributeByIdQuery(Guid Id) : IBaseQuery<CategoryAttributeDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.CategoryAttributeById(Id);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
