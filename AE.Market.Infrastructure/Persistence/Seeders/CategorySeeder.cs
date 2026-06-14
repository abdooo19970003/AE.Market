using AE.Market.Domain.Aggregates.Catalog;

namespace AE.Market.Infrastructure.Persistence.Seeders;

public static class CategorySeeder
{
    public static List<Category> GetSeedData()
    {
        var electronics = Category.Create(Guid.NewGuid(), "Electronics", "electronics", "All electronic items");
        var clothing = Category.Create(Guid.NewGuid(), "Clothing", "clothing", "Apparel and accessories");
        var books = Category.Create(Guid.NewGuid(), "Books", "books", "Books and media");
        var phones = Category.Create(Guid.NewGuid(), "Mobile Phones", "mobile-phones", "Smartphones and accessories", electronics.Id);
        var laptops = Category.Create(Guid.NewGuid(), "Laptops", "laptops", "Notebooks and ultrabooks", electronics.Id);

        return [electronics, clothing, books, phones, laptops];
    }
}
