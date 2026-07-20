using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.CategoryAttributes;

public sealed record GetCategoryAttributesByCategoryQuery(Guid CategoryId) : IBaseQuery<List<CategoryAttributeDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.CategoryAttributesByCategory(CategoryId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
