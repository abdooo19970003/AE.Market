# super-attribute-validation — Draft

## Intent
CLEAR — outcome known: enforce super-attribute validation for Configurable products. The gaps were identified and confirmed through codebase exploration.

## Research summary (evidence)
- `ProductAttributeValue.IsVariantDefiner` (ProductAttributeValue.cs:17) exists but **no domain rule enforces it**
- `Product.Activate()` (Product.cs:180-197) checks for variants on non-Simple/Digital/Bundle products but does NOT check for super-attributes on Configurable
- `Product.UpdateProductType()` (Product.cs:140-146) silently changes type with no validation
- `Product.SetAttributeValue()` (Product.cs:425-470, `internal`) accepts `isVariantDefiner` but no validation enforces it
- `ProductVariant.SetAttributeValue()` (ProductVariant.cs:74-118, `internal`) does NOT accept `isVariantDefiner` (always null)
- `ProductMissingRequiredAttributes` error (CatalogErrors.cs:127-130) exists but is **never thrown anywhere**
- `Product.GetMissingRequiredAttributeIds()` (Product.cs:479-497) exists and works
- **No command exists** to expose `Product.SetAttributeValue()` or `ProductVariant.SetAttributeValue()` to the application layer
- `AddProductVariantCommandValidator` doesn't check parent product is Configurable
- `CreateProductCommandValidator` / `UpdateProductCommandValidator` only validate ProductType is a valid enum name
- `ProductsController.cs` has **no endpoints** for attributes

## Components ledger
1. **Domain rules** — CatalogErrors.cs + Product.cs (Activate, UpdateProductType, SetAttributeValue)
2. **Application commands** — SetProductAttributeValueCommand + SetVariantAttributeValueCommand
3. **Validators** — AddProductVariantCommandValidator, optional Create/UpdateProduct validators  
4. **Wire up ProductMissingRequiredAttributes** — activation flow calls HasMissingRequiredAttributes
5. **API endpoints** — ProductsController attribute routes
6. **Tests** — domain + integration tests

Status: awaiting topology confirmation
