using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.AttributeGroups;

public sealed record GetAttributeGroupByIdQuery(Guid Id) : IBaseQuery<AttributeGroupDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.AttributeGroupById(Id);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(15);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
