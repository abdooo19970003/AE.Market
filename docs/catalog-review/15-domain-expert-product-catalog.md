# Domain Expert Evaluation: Product & Catalog Core Domain Modeling

## Executive Summary

The Catalog aggregate demonstrates a well-structured domain-driven design with strong encapsulation, rich domain events, and thoughtful handling of product types, variants, attribute metadata, and stock operations. The aggregate root (`Product`) owns its child entities (`ProductVariant`, `ProductImage`, `Tag`, `ProductAttributeValue`, `ProductRelation`) with proper invariants enforced at every mutation point. However, several real-world e-commerce concepts remain unmodeled or immature: pricing is purely reactive (computed from variants with no product-level price), bundle/configurable product structures lack necessary sub-models, inventory is single-location-only, and there is no multilingual/content localization support. Additionally, some domain logic (attribute validation, brand/category name resolution) leaks into the application layer, and critical invariants like "a simple product must always have a sale price" are unenforced.

---

## PROS

### Business concepts well-captured

- **Product types enumeration** (`ProductType.cs`: Simple, Configurable, Bundle, Digital) -- correctly distinguishes four fundamental merchandising categories, enabling type-specific behavior.
- **Product variants with full stock lifecycle** (`ProductVariant.cs:9-20`, `Product.cs:297-340`) -- the variant entity includes `StockQuantity`, `ReservedQuantity`, `AvailableQuantity` (computed), `RowVersion`, and methods for setting/adjusting/reserving/releasing stock. This supports order fulfillment workflows well.
- **Dual pricing on variants** (`ProductVariant.cs:16-17`) -- both `SalePrice` (actual selling price) and `ListPrice` (MSRP/original price) are modeled, enabling discount/compare-at display.
- **Backorder support with configurable limit** (`Product.cs:29-30`, `Product.cs:267-272`) -- `AllowBackOrder` + `BackOrderLimit` at product level, with the business logic that clearing `AllowBackOrder` also nullifies the limit.
- **Product relations taxonomy** (`RelationType.cs`) -- four relation types (Related, CrossSell, UpSell, Accessory) plus self-relation prevention (`Product.cs:276-277`) and duplicate-same-type detection (`Product.cs:279`).
- **Image management with primary/sort** (`ProductImage.cs:9-10`, `Product.cs:202-206`) -- only one image can be primary (auto-demotion of others), plus explicit sort ordering.
- **EAV attribute system** (`ProductAttributeValue.cs`, `VariantAttributeValue.cs`, `CategoryAttribute.cs`) -- full custom-attribute infrastructure with type validation (`AttributeInputTypeValidator`), option lists for multi-select attributes, and unit-of-measure support.
- **SEO metadata** (`IMetaData.cs`, `Product.cs:33-35`, `ProductVariant.cs:29-33`) -- `MetaTitle`, `MetaDescription`, `MetaKeywords` on Product, ProductVariant, Brand, and Category.
- **Category attribute inheritance** (`Category.cs:139-156`) -- `GetEffectiveAttributes()` traverses ancestors to compute the full attribute set, leaking no logic to the application layer.

### Invariants correctly enforced

- **Cannot activate a non-simple/digital product without variants** (`Product.cs:159-160`, `Product.cs:425-426`) -- `Activate()` and `Restore()` both check that Configurable/Bundle products have at least one variant before activating.
- **Cannot remove the last variant from an active product** (`Product.cs:186-187`, `CatalogErrors.CannotRemoveLastVariant`).
- **Self-relation prevention** (`Product.cs:276-277`, `CatalogErrors.ProductCannotRelateToSelf`).
- **Insufficient stock protection** (`ProductVariant.cs:131-132, 138-140, 146-147`) -- `SetQuantity`, `AdjustStock`, and `ReserveStock` all guard against negative stock or over-reservation.
- **Category circular parent reference prevention** (`Category.cs:112-116`) -- cannot become own child or own descendant.
- **BackOrderLimit only set when backorder allowed** (`Product.cs:269-270`).
- **Slug and SKU value object validation** (`Slug.cs`, `Sku.cs`) -- regex-validated with normalization.
- **Required attribute validation API** (`Product.cs:387-403`) -- `GetMissingRequiredAttributeIds` and `HasAllRequiredAttributes` provide query methods for invariants, though their enforcement is left to callers.

### Edge cases handled well

- **Idempotent tag addition** (`Product.cs:224`) -- `AddTag` silently returns if the slug already exists rather than throwing.
- **Duplicate relation handling** (`Product.cs:279-280`) -- `AddRelation` returns the existing relation instead of erroring or creating a duplicate.
- **Stock release never goes negative** (`ProductVariant.cs:154`) -- uses `Math.Max(0, ...)`.
- **Soft delete cascades to all children** (`Product.cs:406-421`, `ProductVariant.cs:164-172`) -- deletes all variants, images, tags, attribute values, and relations.
- **Idempotent activate/deactivate** (`Product.cs:156-158, 166-168`) -- no-ops if already in the desired state.

### Ubiquitous Language consistency

- Domain terminology is internally consistent: "Variant" (not "SKU" or "Option"), "Relation" (not "Cross-sell Link"), "Attribute/AttributeValue" (not "Property/PropertyValue"), "Category" (not "Department"), "Brand" (not "Manufacturer").
- The `IsPurchasable` computed property (`Product.cs:21-23`) is a nice domain concept combining active status and variant availability.
- Error codes in `CatalogErrors.cs` follow a consistent `Catalog.{Entity}.{Issue}` naming pattern.

---

## GAPS

### Missing business concepts

| Gap | Severity | Location | Description |
|-----|----------|----------|-------------|
| **No product-level sale price for Simple/Digital types** | **HIGH** | `Product.cs:57` -- `SalePrice` computed from variants; `Product.cs:81-94` -- `Create()` creates no variant | A Simple or Digital product has no intrinsic price if no variant is added. The computed `SalePrice` returns 0 for a freshly created Simple product, which is never valid in commerce. Simple/Digital products should require a price at creation or have a product-level price field. |
| **No bundle product model** | **HIGH** | `ProductType.cs:7` -- Bundle enum exists but no `BundleComponent` entity | Bundle products have no way to define which products comprise the bundle, at what quantities, or with what pricing rules. The `Bundle` product type is a stub. |
| **No configurable product attribute mapping** | **HIGH** | `ProductType.cs:6` -- Configurable enum exists but no super-attribute model | Configurable products (e.g., T-shirt in multiple colors/sizes) need a concept of "super attributes" that define the variant grid. The current `VariantAttributeValue` system doesn't distinguish between descriptive attributes and variant-defining attributes. |
| **No per-variant backorder control** | **MEDIUM** | `Product.cs:29-30` -- backorder fields on Product, not Variant | In real e-commerce, one variant may be on backorder while another is in stock. Backorder settings should be per-variant (inheriting from product by default). |
| **No per-warehouse/multi-location inventory** | **MEDIUM** | `ProductVariant.cs:18-20` -- single stock quantity | Only a total `StockQuantity` is tracked. No support for warehouse-specific stock, transfer orders, or location-based availability. |
| **No inventory status enumeration** | **MEDIUM** | `ProductVariant.cs:18` -- just an integer | Common statuses like "InStock", "OutOfStock", "Discontinued", "PreOrder" are not modeled. The `IsActive` flag conflates "product is visible" with "product is available." |
| **No product visibility/publish scheduling** | **MEDIUM** | `Product.cs:19` -- only a boolean `IsActive` | No `PublishedAt`/`ScheduledFrom`/`ScheduledTo` for timed product launches or seasonal availability. |
| **No currency support** | **MEDIUM** | `ProductVariant.cs:16-17` -- prices are raw decimals | Prices lack a currency context. In a multi-currency marketplace, this is a significant gap. |
| **No localized/translated content** | **LOW** | `Product.cs:13-18` -- name/descriptions are single strings | Product names, descriptions, slugs, and SEO fields cannot support multiple languages. |
| **No "Grouped Product" type** | **LOW** | `ProductType.cs` -- missing from enum | Common e-commerce pattern (e.g., a "Collection" of simple products sold together or as options). |
| **Tags are not shareable** | **MEDIUM** | `Tag.cs` -- Tag is an owned child entity | Tags are created per-product (`Product.AddTag` creates a new Tag instance). There's no shared tag catalog, so tag-based filtering across products or tag management UIs are impossible. |
| **No variant-level domain events for price/stock changes** | **MEDIUM** | `ProductVariant.cs` -- no `AddDomainEvent` calls | Price changes (`SetOrUpdateSellingPrice`), activation/deactivation, and stock adjustments on the variant itself raise no domain events. Events are only raised at the Product level (`VariantPriceChangedDomainEvent`, `VariantStockAdjustedDomainEvent`), but the variant mutation methods themselves are silent. |
| **No published/scheduled domain events** | **LOW** | Product events | There is no `ProductPublishedDomainEvent` or `ProductVisibilityChangedDomainEvent` for when a product transitions between scheduled/draft/published states. |

### Missing business rules

| Rule | Severity | Location | Description |
|------|----------|----------|-------------|
| **Simple/Digital products must always have at least one variant** | **HIGH** | `Product.cs:81-94` | `Product.Create()` does not enforce or create an initial variant. A Simple product with no variants has zero computed `SalePrice` and zero `StockQuantity`. Should either require a variant at creation or auto-create one. |
| **SalePrice must be greater than zero for active variants** | **MEDIUM** | `ProductVariant.cs:16` -- no validation | There's no invariant that a variant's `SalePrice` must be > 0 when the variant is active. |
| **ListPrice must be >= SalePrice** | **LOW** | `ProductVariant.cs:16-17` -- no cross-field validation | Common business rule that the list (original) price should be at least the sale price. |
| **Required attributes for product activation** | **MEDIUM** | `Product.cs:155-163` -- `Activate()` checks variants exist but not required attributes | Even though `CatalogErrors.ProductMissingRequiredAttributes` exists and `HasAllRequiredAttributes` is queryable, the `Activate()` method does not enforce that required attributes are set. It should call `HasAllRequiredAttributes` internally. |
| **No uniqueness validation on variant SKUs within a product** | **LOW** | `Product.cs:175-182` -- `AddVariant` does not check SKU uniqueness | SKUs should be unique at the global level (enforced by the `Sku` value object pattern) but also at the product level for user experience. |

### Incomplete workflows

| Workflow | Severity | Description |
|----------|----------|-------------|
| **Product creation flow** | **MEDIUM** | `CreateProductCommandHandler.cs` creates a product then immediately calls `UpdateBrand`, `UpdateTaxCode`, etc. as separate steps. This generates multiple domain events for what could be a single atomic creation event. A richer factory method or builder pattern could consolidate creation. |
| **Product deletion flow** | **MEDIUM** | `DeleteProductCommandHandler.cs:19` calls `repo.Delete(product)` but never calls `product.Delete()`. The domain's soft-delete cascade logic in `Product.Delete()` is bypassed. |
| **Restore ignores children** | **MEDIUM** | `Product.cs:423-429` -- `Restore()` reactivates product but does not restore deleted children (variants, images, etc.). |

### Missing domain events

| Missing Event | Severity | Reason |
|---------------|----------|--------|
| `ProductAttributeValueSetDomainEvent` | MEDIUM | Product.cs:354-378 -- setting an attribute value raises no domain event |
| `ProductAttributeValueRemovedDomainEvent` | MEDIUM | Product.cs:380-385 -- removing an attribute value raises no domain event |
| `VariantActivatedDomainEvent` / `VariantDeactivatedDomainEvent` | LOW | ProductVariant.cs:57-69 -- variant activate/deactivate methods raise no events |
| `ProductImageSetPrimaryDomainEvent` | LOW | ProductImage.cs:39-43 -- promoting an image to primary raises no event |
| `ProductAllowBackOrderChangedDomainEvent` | LOW | Product.cs:267-272 -- changing backorder settings raises no event |

---

## ISSUES

### Anemic domain model / logic leakage

| Issue | Severity | Location | Description |
|-------|----------|----------|-------------|
| **Required attribute validation leaks to application layer** | **HIGH** | `Product.cs:387-403` vs application handlers | `GetMissingRequiredAttributeIds` requires external callers to pass the required set. The domain should be able to look this up from the Category. Instead, `CreateProductCommandHandler` and `Activate` have no attribute validation. |
| **Brand/Category name resolution leaks to queries** | **MEDIUM** | `GetProductByIdQueryHandler.cs:31-43` | Product queries manually fetch brand and category names via separate repo calls. This should be handled by the query infrastructure (includes, projections, or read models). |
| **ProductDto `ProductType` is a string** | **LOW** | `ProductDto.cs:19`, `CreateProductCommandHandler.cs:17` | Product type is round-tripped through string parsing instead of using the domain enum, risking runtime errors. |
| **Variant lookup duplicated across handlers** | **MEDIUM** | Multiple handlers: AdjustVariantStock, ReserveVariantStock, UpdateVariantPricing, etc. | Every stock/pricing handler repeats `product.Variants.FirstOrDefault(v => v.Id == request.VariantId)` with identical null-check logic. This is procedural leakage from the domain into the application layer. |
| **DomainException caught and converted to Result in application layer** | **MEDIUM** | `AdjustVariantStockCommandHandler.cs:27-33`, `ReserveVariantStockCommandHandler.cs:27-33` | Domain exceptions should not need try/catch wrapping in the application layer. The domain methods should return `Result` instead of throwing. |

### Ubiquitous Language inconsistencies

| Issue | Severity | Location | Description |
|-------|----------|----------|-------------|
| **`CategoryName` vs `Name` vs `GroupUnitName`** | **LOW** | `Category.cs:12`, `Product.cs:13`, `Brand.cs:9`, `GroupUnit.cs:10` | Inconsistent naming: Category uses `CategoryName`, GroupUnit uses `GroupUnitName`, but Product and Brand use `Name`. |
| **`ProductDetailsUpdatedDomainEvent` sends full state but later mutations send diffs** | **LOW** | `ProductDetailsUpdatedDomainEvent.cs` vs `ProductSlugChangedDomainEvent.cs` | Some events carry the full new value (`ProductDetailsUpdatedDomainEvent`: Name + Details), others carry old + new (`ProductSlugChangedDomainEvent`: OldSlug + NewSlug), others carry only old (`ProductCategoryChangedDomainEvent`: OldCategoryId + NewCategoryId). Inconsistent auditing semantics. |
| **`DomainEvent` suffix inconsistency** | **LOW** | `ProductCreatedEvent.cs` vs `ProductActivatedDomainEvent.cs` vs `CategoryCreatedEvent.cs` vs `BrandDeletedEvent.cs` | Some files omit the `DomainEvent` suffix while others include it. E.g., `ProductCreatedEvent` vs `ProductActivatedDomainEvent`. |
| **"Meta fields" vs "SEO" comments** | **LOW** | `Product.cs:32` -- comment says `// SEO` but method is `SetOrUpdateMetaFields` | Minor language inconsistency between the comment convention and the method naming convention. |

### Overly complex or under-designed relationships

| Issue | Severity | Location | Description |
|-------|----------|----------|-------------|
| **Product-level `SalePrice` and `StockQuantity` are computed, forcing unnecessary eager loading** | **MEDIUM** | `Product.cs:57-58`, `GetProductsListQueryHandler.cs` | The computed properties aggregate over variants. The `GetProductsListQuery` spec does NOT include variants (reasonable for performance), which means `SalePrice` and `StockQuantity` are always 0 in product listing queries. This is a proven bug: list queries will show $0 prices and 0 stock for all products. |
| **`VariantDto` omits `ListPrice`** | **MEDIUM** | `VariantDto.cs:10`, `ProductVariant.cs:17` | ListPrice is modeled in the domain but completely absent from the DTO. Cannot display "compare at" pricing. |
| **`ProductDetailDto.Images` is `List<string>` not rich type** | **LOW** | `ProductDetailDto.cs:30` | Images are flattened to URLs, losing `AltText`, `IsPrimary`, and `SortOrder`. |
| **Variant `RowVersion` stored but never checked in domain** | **LOW** | `ProductVariant.cs:21` | `RowVersion` exists as a field but no domain method validates it. Concurrency control depends entirely on EF Core's `SaveChanges` behavior. |

### Aggregate boundary issues

| Issue | Severity | Location | Description |
|-------|----------|----------|-------------|
| **Tag should be a separate aggregate or value object for sharing** | **MEDIUM** | `Tag.cs` | Tags are created per-product with new IDs, making tag-based search, faceted navigation, and tag management impossible. A shared tag catalog (separate aggregate) or tag-as-value-object would be more appropriate. |
| **`ProductTaxCode` is a separate aggregate root but only referenced by ID** | **LOW** | `ProductTaxCode.cs` | This is fine architecturally, but the weak reference pattern means there's no referential integrity guarantee at the domain level. |

---

## Recommendations (Top 5 Actionable Improvements)

1. **Fix the product listing price bug (CRITICAL)**
   - `Product.cs:57-58` -- The computed `SalePrice` and `StockQuantity` aggregate over variants. Since listing queries do not load variants, these computed properties silently return 0. Solution options:
     a) Add a materialized `Product.SalePrice` column that is set/updated when variants change (denormalization).
     b) Ensure listing specs always eager-load variants (performance trade-off).
     c) Use a database-level computed column or projection.
   - **Files**: `Product.cs:57-58`, `GetProductsListQueryHandler.cs`, `ProductByIdSpec.cs`

2. **Mandate an initial variant for Simple/Digital products on creation**
   - `Product.cs:81-94` -- The `Product.Create()` factory method should either (a) accept initial variant parameters and auto-create a variant, or (b) the `CreateProductCommandHandler` should be refactored to perform this as a single atomic operation. Without this, a Simple product exists in an invalid state (zero price, zero stock) until someone manually adds a variant.
   - **Files**: `Product.cs:81-94`, `CreateProductCommandHandler.cs`

3. **Add Bundle components and Configurable super-attribute models**
   - `ProductType.cs` currently has `Bundle` and `Configurable` as enums but no supporting entities. Create:
     - `BundleComponent` (product ID, quantity, sort order, optional pricing rule) for bundles.
     - Super-attribute linkage between Product attributes and Variant attributes for configurable products, so the system can generate the "choose Color + Size" experience.
   - **Files**: New entities under `Products/`, updates to `Product.cs` management methods.

4. **Introduce product-level pricing and enforce pricing invariants**
   - Add an explicit `Product.SalePrice` and `Product.ListPrice` for Simple/Digital types, distinct from the variant-derived price for Configurable types.
   - Enforce in domain invariants: `SalePrice > 0` when `IsActive`, `ListPrice >= SalePrice`.
   - Expose `ListPrice` in `VariantDto.cs` and `ProductDto.cs`.
   - **Files**: `Product.cs`, `ProductVariant.cs`, `VariantDto.cs`, `ProductDto.cs`

5. **Pull required-attribute validation into the domain, and raise attribute change events**
   - Refactor `Product.Activate()` to internally call `HasAllRequiredAttributes()` by accepting the set of required attribute IDs (or better, make the aggregate reference Category so it can look them up).
   - Add domain events: `ProductAttributeValueSetDomainEvent`, `ProductAttributeValueRemovedDomainEvent`.
   - Remove the `try/catch DomainException` pattern from `AdjustVariantStockCommandHandler.cs` and similar handlers by making domain methods return `Result` or using a notification pattern. Alternatively, keep the exception approach but add a global middleware to auto-convert `DomainException` to `Result.Fail`.
   - **Files**: `Product.cs:155-163`, `Product.cs:354-385`, `AdjustVariantStockCommandHandler.cs`, `CatalogErrors.cs`
