using AE.Market.Domain.Aggregates.Catalog.Products;

namespace AE.Market.Infrastructure.Persistence.Seeders;

public static class BrandSeeder
{
    public static List<Brand> GetSeedData()
    {
        var xiaomi = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi", "Innovative technology", sortOrder: 1);
        var samsung = Brand.Create(Guid.NewGuid(), "Samsung", "samsung", "Next generation", sortOrder: 2);
        var nike = Brand.Create(Guid.NewGuid(), "Nike", "nike", "Just do it", sortOrder: 3);

        return [xiaomi, samsung, nike];
    }
}
