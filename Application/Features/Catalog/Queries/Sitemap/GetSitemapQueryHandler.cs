using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Sitemap;

internal sealed class GetSitemapQueryHandler(
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo
) : IRequestHandler<GetSitemapQuery, Result<SitemapDto>>
{
    public async Task<Result<SitemapDto>> Handle(
        GetSitemapQuery request,
        CancellationToken cancellationToken
    )
    {
        var products = await productRepo.ListAsync(cancellationToken);
        var categories = await categoryRepo.ListAsync(cancellationToken);

        var urls = new List<SitemapUrlDto>();

        foreach (var category in categories.Where(c => !c.IsDeleted && c.IsActive))
        {
            urls.Add(new SitemapUrlDto(
                Loc: $"/categories/{category.Slug}",
                LastMod: category.LastModified,
                ChangeFreq: "weekly",
                Priority: 0.7
            ));
        }

        foreach (var product in products.Where(p => !p.IsDeleted && p.Status == ProductStatus.Active))
        {
            urls.Add(new SitemapUrlDto(
                Loc: $"/products/{product.Slug}",
                LastMod: product.LastModified,
                ChangeFreq: "daily",
                Priority: 0.9
            ));
        }

        return Result<SitemapDto>.Success(new SitemapDto(urls));
    }
}
