# Domain Expert Review — Product Classification & Taxonomy

**Reviewer:** Priya — taxonomy design, catalog management, faceted search

---

## 1. Category Hierarchy: Multi-Level Classification Support

The `Category` entity supports a multi-level, tree-based hierarchy through a self-referencing **parent-child** relationship. The `ParentId` (nullable `Guid`) and `ParentCategory` navigation property, paired with the `_subCategories` collection, allow an arbitrary tree depth. The `ChangeParent(Guid?)` method correctly guards against self-parenting (a cycle at depth 1) but does **not** guard against deeper cycles (e.g., moving a node under its own descendant). This is a real-world concern: in a tree of 5+ levels, a naive `ChangeParent` could introduce a cycle that would break recursive queries. A production-grade fix would check that `newParentId` is not a descendant of the current node before allowing the move. Additionally, the `SortOrder` field supports ordered sibling presentation, and `IsActive` allows soft-enable/disable of entire branches. The `Slug` value object is well-constructed with normalization, and `CategoryName` uniqueness is handled at the error level (`CategoryNameAlreadyExists`). On the whole, the hierarchy is sound, and the use of an adjacency-list model (parentId) is standard. However, for very deep trees or high-read-volume workloads, you will eventually want to add **materialized path** or **nested set** columns (e.g., `TreePath`, `Level`) to avoid recursive CTEs on every subtree query. This is not yet present.

From a **classification** standpoint, the model allows a single taxonomy parent, which is the industry norm. One notable omission: there is no `CategoryType` or `CategoryGroup` concept to distinguish *product categories* (e.g., "Electronics > Laptops") from *department-level* or *merchandising* groupings. In large-scale retail, categories often serve dual roles — navigation taxonomy versus attribute-inheritance containers — and having a lightweight type discriminator helps.

## 2. Product Attributes and Attribute Options: Faceted Search Readiness

The attribute subsystem is one of the stronger parts of this model. `CategoryAttribute` is correctly scoped to a `Category` (via `CategoryId`), meaning each category defines its own attribute schema — a standard approach for faceted classification. The `AttributeInputType` enum (`Text`, `Integer`, `Decimal`, `MultiSelect`, `Boolean`, `DateTime`) covers the vast majority of filterable product properties. The `IsFilterable` flag on each attribute is a clear signal for which attributes should appear in faceted navigation. The `AttributeOption` entity (with `Label`, `Value`, `SortOrder`) supports controlled vocabularies for `MultiSelect` inputs — essential for attributes like "Color" or "Size." This enables proper faceted search aggregation: the search engine can group products by `AttributeOption.Value` and return counts.

However, there are significant gaps for a production retail catalog:

- **No attribute inheritance.** Attributes are defined per-category but are **not** automatically inherited by child categories. In a real catalog, "Electronics" may define "Brand" and "Power Source," and all subcategories (e.g., "Laptops," "Cameras") should inherit those attributes. Without this, every child category must manually redefine shared attributes, leading to duplication and inconsistency.
- **No attribute groups.** Even within a single category, attributes should be groupable (e.g., "Physical Dimensions" group: Width, Height, Depth, Weight; "Technical Specs" group: Processor, RAM, Storage). This is missing.
- **AttributeValue is variant-scoped only.** `AttributeValue` lives on `ProductVariant`, not on `Product`. This means product-level attributes (e.g., "Brand," "Manufacturer," "Warranty Length") cannot be stored without creating a dummy variant. A common pattern is to have both `ProductAttributeValue` and `VariantAttributeValue` (or mark some attributes as "shared across variants").
- **No validation bridge.** The `AttributeValue` entity accepts all typed nullable fields (`ValueText`, `ValueInteger`, `ValueDecimal`, etc.) without any domain validation that the provided value matches the `AttributeInputType`. It is possible to set a `ValueInteger` on a `Text`-type attribute. This should be enforced either in `SetAttributeValue` or via a domain service.
- **No attribute metadata beyond display.** There is no metadata for measurement unit (beyond `DefaultUnitId` / `AllowedGroupUnitId`), no search boost weight, no "show in product listing" versus "show in product detail" hints. For a faceted search engine to rank facets intelligently, these hints matter.

## 3. Unit-of-Measure Model: Flexibility for Diverse Product Types

The `GroupUnit` / `Unit` pairing is a well-designed, normalized approach to units of measure. A `GroupUnit` (e.g., "Weight," "Volume," "Length") contains multiple `Unit` instances (e.g., "Kilogram", "Gram", "Pound") with `IsBaseUnit` and `ExchangeRateToBaseUnit` for conversion. This allows the system to store values in a canonical base unit and convert for display. The `DefaultUnitId` and `AllowedGroupUnitId` on `CategoryAttribute` link attributes to the appropriate unit group — for example, a "Weight" attribute would reference the "Weight" group and default to "Kilogram." This is **exactly** how a mature catalog handles dimensional attributes.

A few observations:
- The conversion model assumes linear conversion (multiplication by `ExchangeRateToBaseUnit`). This works for weight, length, volume, but would fail for temperature (offset-based) or other nonlinear scales. A note or extension point for non-linear conversions (e.g., a conversion function ID) would be wise.
- There is no explicit "piece" / "each" / "unit" concept. While you could define a `GroupUnit` called "Count" with a single `Unit` called "Piece" (exchange rate 1), treating countable items as a unit-of-measure group feels unnatural. Many systems separate "sold by unit" (discrete count) from "sold by measure" (continuous quantity).
- The attribute-to-unit binding is one-directional: the attribute knows its unit, but there is no way to convert an attribute *value* at query time unless you join through the `Unit` and `GroupUnit` tables. This is acceptable, but the conversion logic belongs in the application layer (a domain service or a `UnitConverter` helper).

## 4. Major Missing Entities and Structural Gaps

For a retail catalog serving real-world e-commerce, the following are conspicuously absent:

**Brand, Manufacturer, Supplier.** These are foundational to product classification. A `Brand` entity (with its own logo, slug, SEO metadata) is critical for faceted filtering — "Brand" is the single most-used filter in retail. `Manufacturer` is a separate concern (the entity that produces the product). `Supplier` (the entity that supplies inventory) belongs in a purchasing/inventory context but is often weakly referenced from the product. Without these, the catalog cannot answer basic queries like "all products by this brand" or "filter by manufacturer."

**Product Type (or Product Template).** A `ProductType` acts as a schema template that defines which attributes, which unit groups, and which behavior (e.g., "simple product" vs. "variant product" vs. "configurable product") apply. Categories and product types often form a many-to-many relationship. The current model conflates category with attribute schema, making it impossible to reuse the same attribute set across different categories (e.g., "T-Shirts" and "Polo Shirts" are different categories but share the same attributes: Size, Color, Material). A product template would solve this.

**Pricing and Inventory.** While pricing may belong in a separate bounded context (and the AGENTS.md acknowledges this), even a minimal catalog domain should include list price / MSRP as a product attribute. The current model has zero monetary fields on `Product` or `ProductVariant`. Without a price, a product is not tradeable.

**Product Relationships.** There is no model for related products (cross-sell, up-sell, accessories, bundles, variants of the same product line). The `ProductVariant` relationship covers SKU-level variation (size/color), but there is no `ProductGroup` or `LinkedProduct` entity.

**Product Dimensions.** Physical products require weight, length, width, height, and the unit of those dimensions. This is not present on `Product` or `ProductVariant`.

**SEO / Content.** The SEO fields (`MetaTitle`, `MetaDescription`, `MetaKeywords`) are a good start, but there is no field for a rich-text `Description` (the current `Details` is a plain string), no `ShortDescription` for listing pages, and no `SearchKeywords` or tags for full-text search boosting.

**Soft Delete for Catalog Entities.** While `BaseEntity` provides `IsDeleted`, the catalog entities (`Category`, `Product`, `ProductVariant`) expose `IsActive` but do not explicitly use the soft-delete mechanism. The error `CategoryHasProducts` suggests a hard-delete guard, not a soft-delete pattern. For audit and recovery, soft delete on `Category` and `Product` is recommended.

## 5. Overall Assessment

What exists is a **solid, well-structured foundation** for a product catalog domain. The aggregate boundaries are sensible: `Category` is the aggregate root for the taxonomy; `Product` is the aggregate root for sellable items; `ProductVariant` is a child entity of `Product`. The unit-of-measure model is above average for a greenfield project, and the attribute system with typed values and `InputType` shows clear intent to support faceted search. The use of domain events (`CategoryCreatedDomainEvent`, `ProductUpdatedDomainEvent`) aligns with the event-driven architecture described in AGENTS.md.

However, the domain model is **incomplete for a retail catalog**. It is essentially an attribute-driven taxonomy engine with products and variants, but it lacks the core entities (Brand, Manufacturer), the structural scaffolding (Product templates, attribute inheritance, attribute groups), and the commercial data (pricing, dimensions) needed for a real storefront. The application layer (Commands, Queries, DTOs) is all placeholders, so there is no command/query logic to evaluate either. Given the AGENTS.md acknowledgment that "only Auth feature has code; Catalog, Pricing, etc. are stubs," this is consistent with the project's current state. The next development priorities should be (a) Brand and Manufacturer entities, (b) attribute inheritance from parent categories, (c) attribute groups, and (d) a product template system to decouple attribute schemas from individual categories.
