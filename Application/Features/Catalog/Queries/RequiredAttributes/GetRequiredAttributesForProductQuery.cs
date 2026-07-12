using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.RequiredAttributes;

public sealed record GetRequiredAttributesForProductQuery(
    Guid CategoryId,
    Guid? ProductId = null
) : IBaseQuery<List<RequiredAttributeDto>>, ICachedQuery
{
    public string CacheKey => $"required-attributes-c{CategoryId}-p{ProductId}";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
