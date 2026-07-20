using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.CategoryAttributes;

public sealed record GetAttributeGroupsByCategoryQuery(Guid CategoryId) : IBaseQuery<List<AttributeGroupDto>>, ICachedQuery
{
    public string CacheKey => CacheKeys.AttributeGroupsByCategory(CategoryId);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
