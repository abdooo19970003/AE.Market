using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Queries.Sitemap;

public sealed record GetSitemapQuery : IBaseQuery<SitemapDto>, ICachedQuery
{
    public string CacheKey => "sitemap-xml";
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromHours(1);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
