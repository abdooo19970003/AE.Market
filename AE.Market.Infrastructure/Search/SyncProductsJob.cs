using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Search.DTOs;
using AE.Market.Application.Features.Search.EventHandlers;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace AE.Market.Infrastructure.Search;

[DisallowConcurrentExecution]
public class SyncProductsJob(
    AppDbContext dbContext,
    IElasticsearchService esService,
    IReadRepository<Category> categoryRepo,
    IReadRepository<Brand> brandRepo,
    IReadRepository<Product> productRepo,
    ILogger<SyncProductsJob> logger
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting nightly ES re-index");

        var indexedProducts = await ReindexProductsAsync(context.CancellationToken);
        var indexedBrands = await ReindexBrandsAsync(context.CancellationToken);

        logger.LogInformation(
            "Nightly ES re-index complete: {Products} products, {Brands} brands",
            indexedProducts, indexedBrands);
    }

    private async Task<int> ReindexProductsAsync(CancellationToken ct)
    {
        const int batchSize = 100;
        int totalIndexed = 0;
        int offset = 0;

        while (true)
        {
            var products = await dbContext.Products
                .AsNoTracking()
                .Include(p => p.Variants).ThenInclude(v => v.AttributeValues)
                .Include(p => p.Images)
                .Include(p => p.Tags)
                .OrderBy(p => p.Id)
                .Skip(offset)
                .Take(batchSize)
                .ToListAsync(ct);

            if (products.Count == 0) break;

            var documents = new List<ProductDocument>(products.Count);
            foreach (var product in products)
            {
                var doc = await ProductDocumentMapper.MapProductAsync(
                    product, categoryRepo, brandRepo, ct);
                documents.Add(doc);
            }

            totalIndexed += await esService.BulkIndexProductsAsync(documents, ct);
            offset += batchSize;

            if (products.Count < batchSize) break;
        }

        return totalIndexed;
    }

    private async Task<int> ReindexBrandsAsync(CancellationToken ct)
    {
        const int batchSize = 100;
        int totalIndexed = 0;
        int offset = 0;

        while (true)
        {
            var brands = await dbContext.Brands
                .AsNoTracking()
                .OrderBy(b => b.Id)
                .Skip(offset)
                .Take(batchSize)
                .ToListAsync(ct);

            if (brands.Count == 0) break;

            var documents = new List<BrandDocument>(brands.Count);
            foreach (var brand in brands)
            {
                var doc = await BrandDocumentMapper.MapBrandAsync(
                    brand, productRepo, ct);
                documents.Add(doc);
            }

            totalIndexed += await esService.BulkIndexBrandsAsync(documents, ct);
            offset += batchSize;

            if (brands.Count < batchSize) break;
        }

        return totalIndexed;
    }
}
