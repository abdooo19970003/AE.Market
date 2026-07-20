namespace AE.Market.Application.Features.Catalog;

internal static class CacheKeys
{
    // AttributeGroups
    internal static string AttributeGroupById(Guid id) => $"attributegroup-{id}";
    internal static string AttributeGroupsList(int page, int pageSize) => $"attributegroups-list-p{page}s{pageSize}";

    // Attributes
    internal static string CategoryAttributeById(Guid id) => $"categoryattribute-{id}";
    internal static string CategoryAttributesList(int page, int pageSize) => $"categoryattributes-list-p{page}s{pageSize}";
    internal static string CategoryAttributesByCategory(Guid categoryId) => $"categoryattributes-{categoryId}";
    internal static string AttributeGroupsByCategory(Guid categoryId) => $"attributegroups-{categoryId}";

    // BundleItems
    internal static string BundleItemById(Guid id) => $"bundleitem-{id}";
    internal static string BundleItemsList(int page, int pageSize) => $"bundleitems-list-p{page}s{pageSize}";
    internal static string BundleItemsByBundle(Guid bundleId) => $"bundleitems-{bundleId}";

    // ProductImages
    internal static string ProductImageById(Guid id) => $"productimage-{id}";
    internal static string ProductImagesList(int page, int pageSize) => $"productimages-list-p{page}s{pageSize}";
    internal static string ProductImagesByProduct(Guid productId) => $"productimages-{productId}";

    // ProductRelations
    internal static string ProductRelationById(Guid id) => $"productrelation-{id}";
    internal static string ProductRelationsList(int page, int pageSize) => $"productrelations-list-p{page}s{pageSize}";
    internal static string ProductRelationsByProduct(Guid productId) => $"productrelations-{productId}";

    // Tags
    internal static string TagById(Guid id) => $"tag-{id}";
    internal static string TagsList(int page, int pageSize) => $"tags-list-p{page}s{pageSize}";
    internal static string ProductTags(Guid productId) => $"product-tags-{productId}";

    // Units
    internal static string UnitById(Guid id) => $"unit-{id}";
    internal static string UnitsList(int page, int pageSize) => $"units-list-p{page}s{pageSize}";

    // Products
    internal static string ProductById(Guid id) => $"product-{id}";
    internal static string ProductBySlug(string slug) => $"product-slug-{slug}";
    internal static string ProductsByBrand(Guid brandId, int page, int pageSize) => $"products-brand-{brandId}-p{page}s{pageSize}";
    internal static string ProductsByCategory(Guid categoryId, int page, int pageSize) => $"products-category-{categoryId}-p{page}s{pageSize}";
    internal static string ProductsByTag(string tagSlug, int page, int pageSize) => $"products-tag-{tagSlug}-p{page}s{pageSize}";
    internal static string ProductsList(int page, int pageSize) => $"products-list-p{page}s{pageSize}";

    // Categories
    internal static string CategoryById(Guid id) => $"category-{id}";
    internal static string CategoriesList(int page, int pageSize) => $"categories-list-p{page}s{pageSize}";

    // Brands
    internal static string BrandById(Guid id) => $"brand-{id}";
    internal static string BrandsList(int page, int pageSize) => $"brands-list-p{page}s{pageSize}";

    // GroupUnits
    internal static string GroupUnitById(Guid id) => $"groupunit-{id}";
    internal static string GroupUnitsList(int page, int pageSize) => $"groupunits-list-p{page}s{pageSize}";

    // ProductTaxCodes
    internal static string ProductTaxCodeById(Guid id) => $"producttaxcode-{id}";
    internal static string ProductTaxCodesList(int page, int pageSize) => $"producttaxcodes-list-p{page}s{pageSize}";
}
