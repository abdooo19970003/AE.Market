# Project Manager Review — Delivery Assessment

**Reviewer:** James — delivery management, risk assessment, sprint planning

---

## Current State: What Is Done vs. What Is Missing

### Domain Layer — Substantially Complete (~85%)

The Domain aggregates for Catalog are well-architected and mostly finished. The following entities are fully implemented with proper factory methods, encapsulation, domain events, and value objects:

| Entity/File | Status | Notes |
|---|---|---|
| `Product` | Complete | `Create()`, `UpdateDetails()`, `Activate/Deactivate()`, `AddVariant()`, `AddImage()` all raise domain events correctly. Backed by `ProductCreatedDomainEvent` and `ProductUpdatedDomainEvent`. |
| `Category` | Complete | Self-referencing parent via `ParentId`, tree of `SubCategories`, attribute management via `AddAttribute()`. Raises `CategoryCreatedDomainEvent` and `CategoryUpdatedDomainEvent`. |
| `ProductVariant` | Complete | Internal constructor (only created through `Product.AddVariant`), `SetAttributeValue()` with upsert logic, image management. |
| `ProductImage` / `VariantImage` | Complete | Primary image logic, sort order. |
| `AttributeValue` (variant-level) | Complete | Polymorphic value storage (text, integer, decimal, boolean, datetime, optionId). |
| `CategoryAttribute` / `AttributeOption` | Complete | EAV attribute definitions with options. |
| `Unit` / `GroupUnit` | Complete | Measurement unit system with base unit and exchange rates. |
| `Slug` / `Sku` value objects | Complete | Strong validation via regex and `Guard` clauses. |
| `AttributeInputType` enum | Complete | Text, Integer, Decimal, MultiSelect, Boolean, DateTime (though BACKEND_PLAN mentions "select" and "multi_select" as separate, this design merges them). |
| `CatalogErrors` | Complete | Covers all anticipated failure modes: Category/Product/Variant/Attribute not found, slug/SKU conflicts, circular parent reference. |

### Three Domain Gaps Identified

1. **`Product` entity has no `Sku` property.** The BACKEND_PLAN database schema specifies `products.sku` (unique). Currently only `ProductVariant` carries a SKU. This is either an intentional design choice (SKU lives on variants only) or an oversight. It needs clarification with the product owner.

2. **Missing `ProductAttributeValue` entity.** The BACKEND_PLAN Sprint 4 calls for both `ProductAttributeValue` (product-level attributes) and `VariantAttributeValue` (variant-level). Only the variant-level `AttributeValue` exists. Product-level attribute storage is completely absent.

3. **Missing `VariantAddedEvent`.** The BACKEND_PLAN event catalog lists `VariantAddedEvent`, but `Product.AddVariant()` only raises `ProductUpdatedDomainEvent`. This is a minor gap — a dedicated event may be needed if downstream subscribers (e.g., search indexing) need to distinguish variant addition from other product updates.

### Everything Else — 0% Complete

| Layer | Status |
|---|---|
| **Infrastructure (EF Configurations)** | **0%** — No `IEntityTypeConfiguration<T>` files exist for any Catalog entity. The `Configurations/` folder contains only Auth configs. No `DbSet<T>` properties in `AppDbContext`. No migrations for catalog tables. This is the single biggest blocker. |
| **Application Layer** | **0%** — Every feature folder (`Commands/`, `Queries/`, `DTOs/`, `Specs/`) contains only empty `Placeholder.cs` stub files. `CacheKeys` is an empty class. The four event handlers (`ProductCreatedEventHandler`, `ProductUpdatedEventHandler`, `CategoryCreatedEventHandler`, `CategoryUpdatedEventHandler`) are wired with MediatR but return `Task.CompletedTask` — they do nothing. |
| **API Layer** | **0%** — No `CategoriesController`, `ProductsController`, or any Catalog-related controller. Only `AuthController` exists. |
| **Tests** | **0%** — No domain unit tests, no application tests, no integration tests for Catalog. All test files are Auth-only. |

---

## Remaining Work Estimate

Based on the BACKEND_PLAN's sprint breakdown (Sprints 2–4 for Categories, Products & Variants, and Attributes) and the current codebase state, I estimate **2–3 focused sprints (4–6 weeks)** for a full production-ready Catalog feature, assuming a team of 1–2 developers. Here is the breakdown:

### Sprint A (Weeks 1–2) — Foundation: EF Configs + Categories

- Write all EF Core entity configurations: `CategoryConfiguration`, `ProductConfiguration`, `ProductVariantConfiguration`, `ProductImageConfiguration`, `VariantImageConfiguration`, `CategoryAttributeConfiguration`, `AttributeOptionConfiguration`, `AttributeValueConfiguration`, `UnitConfiguration`, `GroupUnitConfiguration`. (~8–10 configuration files, following the `UserConfiguration` pattern)
- Add `DbSet<T>` properties to `AppDbContext` for all Catalog entities.
- Generate EF migration for the `catalog` schema.
- Implement Application layer for Categories: `CreateCategoryCommand` + Handler + Validator, `UpdateCategoryCommand`, `DeleteCategoryCommand`, `GetCategoryTreeQuery`, `GetCategoryBySlugQuery`.
- Mapster DTOs: `CategoryDto`, `CategoryTreeDto`.
- Specification classes: `CategoryBySlugSpec`, `CategoryTreeSpec`, `CategoryHasProductsSpec`.
- `CategoriesController` with all public + admin endpoints.
- Domain unit tests for `Category`.
- Integration tests for category CRUD.

### Sprint B (Weeks 3–4) — Products, Variants, and Attributes

- Resolve domain gaps: add `Product.Sku` property if required; potentially add `ProductAttributeValue` entity.
- Implement Application layer for Products: `CreateProductCommand`, `UpdateProductCommand`, `DeleteProductCommand`, `GetProductQuery`, `GetProductDetailQuery` (full detail with variants + attributes), `ListProductsQuery` (pagination, sorting, filtering by category/text).
- Implement Application layer for Variants: variant CRUD commands (as sub-routes within product commands or separate handlers).
- Implement Application layer for Attributes: attribute definition CRUD, `SetProductAttributeValueCommand`, `SetVariantAttributeValueCommand` (upsert).
- Wire up event handlers: `ProductCreatedEventHandler`, `ProductUpdatedEventHandler` for outbox-to-cache or future search indexing.
- `ProductsController` with all endpoints.
- Fill in `CacheKeys` with proper cache key constants for product/category queries.
- Add `ProductAttributeValueConfiguration` if the entity is added.
- Domain unit tests for `Product`, `ProductVariant`, `ProductImage`, `AttributeValue`.
- Integration tests: create product with variants, paginate product list, filter by category, assign attributes.

### Sprint C (Week 5–6) — Polish, Unit Conversion, and Edge Cases

- Implement the Unit/GroupUnit system if required (this is a nice-to-have for measurement attribute types, but may not be in the MVP).
- Add catalog seed data to `DbSeeder` (sample categories, sample products).
- `GetProductDetailQuery` optimization with split queries for the nested aggregates.
- Finalize caching strategy: mark frequent queries with `ICachedQuery`.
- Complete integration test coverage for all happy paths and error cases.
- Architecture tests to verify Catalog follows the same conventions as Auth.

**Total: ~12–14 weeks of effort for a single developer, or 4–6 weeks with 2 developers working in parallel.**

---

## Risks, Unknowns, and Blockers

### Critical (blocking)

1. **No EF Configuration files exist.** This is the highest-priority blocker. Without `IEntityTypeConfiguration<T>` classes, the database schema cannot be generated. The Auth feature has 4 configuration files; Catalog will need 8–10. This is straightforward work but must be done before anything else can be tested.
2. **AppDbContext lacks Catalog DbSets.** The `AppDbContext` only registers Auth entities. Until Catalog entities are added and the migration is generated, no Catalog data can be persisted.

### High (scoping decisions needed)

3. **Product.Sku — design ambiguity.** The BACKEND_PLAN schema shows `products.sku` (unique), but the `Product` entity does not have a SKU property. Only `ProductVariant` has `Sku`. The team needs a clarification: does the product itself carry a SKU (as the plan says), or is SKU purely a variant-level concept (as the code currently implements)? This affects the database schema, domain logic, and validation.
4. **Product-level attribute storage.** The existing `AttributeValue` is only on `ProductVariant`. If the business needs attributes at the product level (e.g., "Brand" for a product regardless of variant), a new `ProductAttributeValue` entity is needed. This doubles the attribute work.

### Medium

5. **No `VariantAddedEvent`.** If search indexing needs to specifically react to variant additions (separate from general product updates), a new event is needed. Currently `Product.AddVariant()` only raises `ProductUpdatedDomainEvent`.
6. **Unit/GroupUnit system — scope question.** The Unit/GroupUnit entities are complete in the Domain but unused anywhere. Are these for the Catalog MVP or a future enhancement? They add configuration and migration complexity.
7. **No Catalog seed data.** `DbSeeder` only seeds Auth users. Without sample categories and products, development and testing of Catalog endpoints require manual setup.
8. **Architecture compliance risk.** The Catalog domain uses `internal` constructors on child entities (ProductVariant, Unit, etc.) and follows the same patterns as Auth, so compliance should be high. However, the Architecture Tests project should be updated to include Catalog entities to verify this automatically.

---

## Next Sprint Priorities

### Top Priority: Infrastructure Foundation (EF Configs + Migrations)

The single most impactful task is creating the EF Core configuration files. Here is the exact order I recommend:

1. **Create `AE.Market.Infrastructure/Persistence/Configurations/Catalog/` folder** and build entity configurations for all 10 Catalog entities. The `UserConfiguration` in Auth is the template — use `ToTable("table_name", "catalog")`, configure value object conversions for `Slug` and `Sku`, set up the self-referencing `Category.ParentId` relationship, configure the `Product` → `ProductVariant` cascade, and configure `AttributeValue`'s polymorphic columns.
2. **Register DbSets in `AppDbContext`** — add `DbSet<Product>`, `DbSet<Category>`, `DbSet<ProductVariant>`, `DbSet<ProductImage>`, `DbSet<VariantImage>`, `DbSet<CategoryAttribute>`, `DbSet<AttributeOption>`, `DbSet<AttributeValue>`, `DbSet<Unit>`, `DbSet<GroupUnit>`.
3. **Generate the initial Catalog migration** using `dotnet ef migrations add AddCatalogSchema`.
4. **Resolve the Product.Sku ambiguity** with the product owner/architect before coding the configuration.
5. **Immediately after the migration runs: implement the Categories feature fully** (Application commands/queries + Controller) since it is the simplest and has no dependencies on other Catalog entities. This gives the team a working vertical slice to build confidence before tackling Products.

### High Priority for Sprint +1

- Product + Variant Application layer (commands, queries, validators, specs, DTOs)
- ProductsController
- Domain tests for all Catalog entities (modeled after the Auth `UserTests.cs` patterns)

### Deferred

- Unit/GroupUnit system (unless explicitly required for MVP)
- ProductAttributeValue (deferred until product-level EAV is confirmed as needed)
- Cache key constants and caching behavior (can be added incrementally)
- Integration tests (essential but follow after the Application layer is functional)

---

The Catalog feature has a strong Domain foundation — the entities are well-encapsulated, use the Result pattern correctly, raise domain events consistently, and follow the established code conventions. The critical path is the Infrastructure and Application layers, which are entirely unwritten. With focused effort on the EF configurations and a vertical-slice-first approach (ship Categories end-to-end first), the team can deliver a production-ready Catalog in approximately three sprints.
