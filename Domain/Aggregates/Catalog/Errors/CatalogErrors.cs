using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Errors;

public static class CatalogErrors
{
    public static readonly Error CategoryNotFound = new(
        "Application.NotFound.Category",
        "The specified category was not found."
    );

    public static readonly Error CategoryNameAlreadyExists = new(
        "Application.Conflict.Category.NameAlreadyExists",
        "A category with this name already exists."
    );

    public static readonly Error CategoryHasProducts = new(
        "Application.Validation.Category.HasProducts",
        "Cannot delete category that still has products."
    );

    public static readonly Error CategoryCannotBeOwnChild = new(
        "Application.Validation.Category.CannotBeOwnChild",
        "A category cannot be its own parent."
    );

    public static readonly Error CategoryCannotBeOwnDescendant = new(
        "Application.Validation.Category.CannotBeOwnDescendant",
        "A category cannot be a parent of its own descendant."
    );

    public static readonly Error ProductNotFound = new(
        "Application.NotFound.Product",
        "The specified product was not found."
    );

    public static readonly Error ProductSlugAlreadyExists = new(
        "Application.Conflict.Product.SlugAlreadyExists",
        "A product with this slug already exists."
    );

    public static readonly Error VariantNotFound = new(
        "Application.NotFound.Variant",
        "The specified variant was not found."
    );

    public static readonly Error ProductSkuAlreadyExists = new(
        "Application.Conflict.Product.SkuAlreadyExists",
        "A product with this SKU already exists."
    );

    public static readonly Error VariantSkuAlreadyExists = new(
        "Application.Conflict.Variant.SkuAlreadyExists",
        "A variant with this SKU already exists."
    );

    public static readonly Error AttributeNotFound = new(
        "Application.NotFound.Attribute",
        "The specified attribute was not found."
    );

    public static readonly Error AttributeOptionNotFound = new(
        "Application.NotFound.AttributeOption",
        "The specified attribute option was not found."
    );

    public static readonly Error BrandNotFound = new(
        "Application.NotFound.Brand",
        "The specified brand was not found."
    );

    public static readonly Error TaxCodeNotFound = new(
        "Application.NotFound.TaxCode",
        "The specified tax code was not found."
    );

    public static readonly Error GroupUnitNotFound = new(
        "Application.NotFound.GroupUnit",
        "The specified group unit was not found."
    );

    public static readonly Error ProductRelationNotFound = new(
        "Application.NotFound.ProductRelation",
        "The specified product relation was not found."
    );

    public static readonly Error DuplicateProductRelation = new(
        "Application.Conflict.ProductRelation.Duplicate",
        "A relation of this type to the specified product already exists."
    );

    public static readonly Error ProductNoVariants = new(
        "Application.Validation.Product.NoVariants",
        "A product must have at least one variant to be activated."
    );

    public static readonly Error CannotRemoveLastVariant = new(
        "Application.Validation.Product.CannotRemoveLastVariant",
        "Cannot remove the last variant from an active product."
    );

    public static readonly Error AttributeValueTypeMismatch = new(
        "Application.Validation.AttributeValue.TypeMismatch",
        "The provided value does not match the attribute's input type."
    );

    public static readonly Error AttributeAlreadyDefinedOnParent = new(
        "Application.Conflict.Attribute.AlreadyDefinedOnParent",
        "This attribute is already defined on a parent category. Override it instead."
    );

    public static readonly Error AttributeGroupNotFound = new(
        "Application.NotFound.AttributeGroup",
        "The specified attribute group was not found."
    );

    public static readonly Error GroupUnitAlreadyHasBaseUnit = new(
        "Application.Validation.GroupUnit.AlreadyHasBaseUnit",
        "This group unit already has a base unit defined."
    );

    public static readonly Error AttributeOptionDuplicateValue = new(
        "Application.Conflict.Attribute.OptionDuplicateValue",
        "An option with this value already exists for this attribute."
    );

    public static readonly Error UnitExchangeRateInvalid = new(
        "Application.Validation.Unit.ExchangeRateInvalid",
        "The exchange rate must be greater than zero."
    );

    public static readonly Error ProductMissingRequiredAttributes = new(
        "Application.Validation.Product.MissingRequiredAttributes",
        "Cannot activate product. One or more required attribute values are missing."
    );

    public static readonly Error VariantMissingRequiredAttributes = new(
        "Application.Validation.Variant.MissingRequiredAttributes",
        "Cannot activate variant. One or more required attribute values are missing."
    );

    public static readonly Error InsufficientStock = new(
        "Application.Validation.Variant.InsufficientStock",
        "Insufficient stock to reserve the requested quantity."
    );

    public static readonly Error ProductCannotRelateToSelf = new(
        "Application.Validation.Product.CannotRelateToSelf",
        "A product cannot create a relation to itself."
    );
    public static readonly Error CannotAddBundleItemToNonBundleProduct = new(
        "Application.Validation.Product.CannotAddBundleItemToNonBundleProduct",
        "Can not Add Bundle Item To Non Bundle Product"
    );
    public static readonly Error ProductMissingSuperAttributes = new(
        "Application.Validation.Product.MissingSuperAttributes",
        "Configurable products must define at least one variant-defining (super) attribute."
    );
    public static readonly Error CannotRemoveLastSuperAttribute = new(
        "Application.Validation.Product.CannotRemoveLastSuperAttribute",
        "Cannot remove the last variant-defining attribute from a configurable product."
    );
    public static readonly Error SuperAttributeNotAllowedForProductType = new(
        "Application.Validation.Product.SuperAttributeNotAllowedForProductType",
        "Variant-defining (super) attributes can only be set on configurable products."
    );
    public static readonly Error ProductTypeRequiresSuperAttributes = new(
        "Application.Validation.Product.ProductTypeRequiresSuperAttributes",
        "Cannot change product type to Configurable without at least one variant-defining attribute."
    );
    public static readonly Error BundleItemNotFound = new(
        "Application.NotFound.BundleItem",
        "Bundle item not found"
    );
    public static readonly Error BundleProductHasNoItems = new(
        "Application.Validation.Product.BundleProductHasNoItems",
        "A Bundle product must have at least one bundle item."
    );
}
