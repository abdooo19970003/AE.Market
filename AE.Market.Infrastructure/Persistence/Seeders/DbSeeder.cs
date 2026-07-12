using AE.Market.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Infrastructure.Persistence.Seeders;

public sealed class DbSeeder(AppDbContext db, IPasswordService passwordService)
{
    public async Task SeedAsync()
    {
        if (await db.Users.AnyAsync())
            return;

        db.Users.AddRange(UserSeeder.GetSeedData(passwordService));
        await db.SaveChangesAsync();

        await SeedCatalogAsync();
    }

    private async Task SeedCatalogAsync()
    {
        db.Categories.AddRange(CategorySeeder.GetSeedData());
        db.Brands.AddRange(BrandSeeder.GetSeedData());
        db.ProductTaxCodes.AddRange(ProductTaxCodeSeeder.GetSeedData());
        db.GroupUnits.AddRange(GroupUnitSeeder.GetSeedData());

        await db.SaveChangesAsync();
    }
}
