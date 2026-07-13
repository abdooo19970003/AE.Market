using AE.Market.Domain.Aggregates.Pricing;
using AE.Market.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Infrastructure.Persistence.Seeders;

public static class MarketplaceSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Marketplaces.AnyAsync())
            return;

        var globalMarketplace = Marketplace.Create(
            Guid.NewGuid(), "global", "global", Currency.USD);

        db.Marketplaces.Add(globalMarketplace);
        await db.SaveChangesAsync();

        var taxCodes = await db.ProductTaxCodes.Take(3).ToListAsync();
        foreach (var taxCode in taxCodes)
        {
            var taxRate = MarketplaceTaxRate.Create(
                Guid.NewGuid(),
                globalMarketplace.Id,
                taxCode.Id,
                0.20m);
            db.MarketplaceTaxRates.Add(taxRate);
        }

        await db.SaveChangesAsync();
    }
}
