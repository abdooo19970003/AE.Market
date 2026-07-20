using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Analytics.DTOs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Aggregates.Analytics;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Infrastructure.Persistence.Repository;

internal sealed class AnalyticsReadRepository(AppDbContext db) : IAnalyticsReadRepository
{
    public async Task<AdminStatsDto> GetAdminStatsAsync(CancellationToken cancellationToken)
    {
        var totalProducts = await db.Products
            .CountAsync(cancellationToken);

        var activeProducts = db.Products
            .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active);

        var activeStock = await db.ProductVariants
            .Where(v => !v.IsDeleted && activeProducts.Any(p => p.Id == v.ProductId))
            .SumAsync(v => v.StockQuantity, cancellationToken);

        var activePricePerProduct = db.ProductVariants
            .Where(v => !v.IsDeleted && v.Status == ProductStatus.Active && v.ListPrice > 0
                && activeProducts.Any(p => p.Id == v.ProductId))
            .GroupBy(v => v.ProductId)
            .Select(g => g.Min(v => v.ListPrice));

        var averagePrice = await activePricePerProduct
            .AverageAsync(p => (decimal?)p, cancellationToken) ?? 0m;

        var totalCategories = await db.Categories
            .CountAsync(cancellationToken);

        var productsByCategory = await db.Categories
            .Where(c => !c.IsDeleted)
            .Select(c => new CategoryProductCountDto(
                c.CategoryName,
                db.Products.Count(p => p.CategoryId == c.Id && !p.IsDeleted)))
            .ToListAsync(cancellationToken);

        return new AdminStatsDto(
            totalProducts,
            activeStock,
            averagePrice,
            totalCategories,
            productsByCategory);
    }

    public async Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(
        int days,
        int top,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        return await db.Products
            .Where(p => !p.IsDeleted && p.CreatedAt >= cutoff)
            .OrderByDescending(p => p.ViewCount)
            .Take(top)
            .Select(p => new TopProductDto(p.Id, p.Name, p.ViewCount))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TopSearchDto>> GetTopSearchesAsync(
        int days,
        int top,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        return await db.SearchAnalytics
            .Where(s => s.SearchedAt >= cutoff)
            .GroupBy(s => s.SearchText)
            .Select(g => new TopSearchDto(
                g.Key,
                g.Count(),
                g.Average(x => (double)x.LatencyMs),
                g.Max(x => x.SearchedAt)))
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(cancellationToken);
    }
}
