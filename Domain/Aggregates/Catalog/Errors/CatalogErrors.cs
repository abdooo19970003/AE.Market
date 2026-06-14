using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Errors;

public static class CatalogErrors
{
    public static readonly Error CategoryNotFound = new(
        "Catalog.Category.NotFound",
        "The specified category was not found."
    );

    public static readonly Error CategoryNameAlreadyExists = new(
        "Catalog.Category.NameAlreadyExists",
        "A category with this name already exists."
    );

    public static readonly Error CategoryHasProducts = new(
        "Catalog.Category.HasProducts",
        "Cannot delete category that still has products."
    );

    public static readonly Error CategoryCannotBeOwnChild = new(
        "Catalog.Category.CannotBeOwnChild",
        "A category cannot be its own parent."
    );

    public static readonly Error CategoryCannotBeOwnDescendant = new(
        "Catalog.Category.CannotBeOwnDescendant",
        "A category cannot be a parent of its own descendant."
    );

    public static readonly Error ProductNotFound = new(
        "Catalog.Product.NotFound",
        "The specified product was not found."
    );

    public static readonly Error ProductSlugAlreadyExists = new(
        "Catalog.Product.SlugAlreadyExists",
        "A product with this slug already exists."
    );

    public static readonly Error VariantNotFound = new(
        "Catalog.Variant.NotFound",
        "The specified variant was not found."
    );

    public static readonly Error VariantSkuAlreadyExists = new(
        "Catalog.Variant.SkuAlreadyExists",
        "A variant with this SKU already exists."
    );

    public static readonly Error AttributeNotFound = new(
        "Catalog.Attribute.NotFound",
        "The specified attribute was not found."
    );

    public static readonly Error AttributeOptionNotFound = new(
        "Catalog.Attribute.OptionNotFound",
        "The specified attribute option was not found."
    );

    public static readonly Error BrandNotFound = new(
        "Catalog.Brand.NotFound",
        "The specified brand was not found."
    );

    public static readonly Error TaxCodeNotFound = new(
        "Catalog.TaxCode.NotFound",
        "The specified tax code was not found."
    );

    public static readonly Error GroupUnitNotFound = new(
        "Catalog.GroupUnit.NotFound",
        "The specified group unit was not found."
    );

    public static readonly Error ProductRelationNotFound = new(
        "Catalog.ProductRelation.NotFound",
        "The specified product relation was not found."
    );

    public static readonly Error DuplicateProductRelation = new(
        "Catalog.ProductRelation.Duplicate",
        "A relation of this type to the specified product already exists."
    );

    public static readonly Error ProductNoVariants = new(
        "Catalog.Product.NoVariants",
        "A product must have at least one variant to be activated."
    );

    public static readonly Error CannotRemoveLastVariant = new(
        "Catalog.Product.CannotRemoveLastVariant",
        "Cannot remove the last variant from an active product."
    );

    public static readonly Error AttributeValueTypeMismatch = new(
        "Catalog.AttributeValue.TypeMismatch",
        "The provided value does not match the attribute's input type."
    );

    public static readonly Error AttributeAlreadyDefinedOnParent = new(
        "Catalog.Attribute.AlreadyDefinedOnParent",
        "This attribute is already defined on a parent category. Override it instead."
    );

    public static readonly Error AttributeGroupNotFound = new(
        "Catalog.AttributeGroup.NotFound",
        "The specified attribute group was not found."
    );

    public static readonly Error GroupUnitAlreadyHasBaseUnit = new(
        "Catalog.GroupUnit.AlreadyHasBaseUnit",
        "This group unit already has a base unit defined."
    );

    public static readonly Error AttributeOptionDuplicateValue = new(
        "Catalog.Attribute.OptionDuplicateValue",
        "An option with this value already exists for this attribute."
    );

    public static readonly Error UnitExchangeRateInvalid = new(
        "Catalog.Unit.ExchangeRateInvalid",
        "The exchange rate must be greater than zero."
    );

    public static readonly Error ProductMissingRequiredAttributes = new(
        "Catalog.Product.MissingRequiredAttributes",
        "Cannot activate product. One or more required attribute values are missing."
    );

    public static readonly Error VariantMissingRequiredAttributes = new(
        "Catalog.Variant.MissingRequiredAttributes",
        "Cannot activate variant. One or more required attribute values are missing."
    );

    public static readonly Error InsufficientStock = new(
        "Catalog.Variant.InsufficientStock",
        "Insufficient stock to reserve the requested quantity."
    );

    public static readonly Error ProductCannotRelateToSelf = new(
        "Catalog.Product.CannotRelateToSelf",
        "A product cannot create a relation to itself."
    );
}
