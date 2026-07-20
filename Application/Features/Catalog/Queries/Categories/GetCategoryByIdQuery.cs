using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Categories;

public sealed record GetCategoryByIdQuery(Guid Id, bool IncludeChildren = false) : IBaseQuery<CategoryDto>, ICachedQuery
{
    public string CacheKey => CacheKeys.CategoryById(Id);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(30);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
