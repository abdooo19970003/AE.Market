namespace AE.Market.Application.Features.Catalog;

internal static class CacheKeys
{
    // Products
    internal static string ProductById(Guid id) => $"product-{id}";
    internal static string ProductBySlug(string slug) => $"product-slug-{slug}";
    internal static string ProductsByBrand(Guid brandId) => $"products-brand-{brandId}";
    internal static string ProductsByCategory(Guid categoryId) => $"products-category-{categoryId}";
    internal static string ProductsList => "products-list";

    // Categories
    internal static string CategoryById(Guid id) => $"category-{id}";
    internal static string CategoriesList => "categories-list";

    // Brands
    internal static string BrandById(Guid id) => $"brand-{id}";
    internal static string BrandsList => "brands-list";

    // GroupUnits
    internal static string GroupUnitById(Guid id) => $"groupunit-{id}";
    internal static string GroupUnitsList => "groupunits-list";

    // ProductTaxCodes
    internal static string ProductTaxCodeById(Guid id) => $"producttaxcode-{id}";
    internal static string ProductTaxCodesList => "producttaxcodes-list";
}
