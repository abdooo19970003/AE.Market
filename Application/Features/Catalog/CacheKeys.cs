namespace AE.Market.Application.Features.Catalog;

internal static class CacheKeys
{
    // AttributeGroups
    internal static string AttributeGroupById(Guid id) => $"attributegroup-{id}";
    internal static string AttributeGroupsList => "attributegroups-list";

    // Attributes
    internal static string CategoryAttributeById(Guid id) => $"categoryattribute-{id}";
    internal static string CategoryAttributesList => "categoryattributes-list";

    // BundleItems
    internal static string BundleItemById(Guid id) => $"bundleitem-{id}";
    internal static string BundleItemsList => "bundleitems-list";

    // ProductImages
    internal static string ProductImageById(Guid id) => $"productimage-{id}";
    internal static string ProductImagesList => "productimages-list";

    // ProductRelations
    internal static string ProductRelationById(Guid id) => $"productrelation-{id}";
    internal static string ProductRelationsList => "productrelations-list";

    // Tags
    internal static string TagById(Guid id) => $"tag-{id}";
    internal static string TagsList => "tags-list";

    // Units
    internal static string UnitById(Guid id) => $"unit-{id}";
    internal static string UnitsList => "units-list";

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
