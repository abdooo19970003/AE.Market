# Catalog Aggregate — Comprehensive Gaps & Pros Evaluation (Synthesis)

## Executive Summary

The Catalog aggregate is **surprisingly mature** — contrary to `AGENTS.md`'s claim of "stubs at most," it has full Domain (55 files, 53 events), Application (23 commands, 18 queries, 30+ event handlers), Infrastructure (14 EF configs, 5 seeders), API (5 controllers), and Domain Tests (20 files). However, it has **critical production blockers**: an EF config that references a non-existent property (`ShippingDimensions` on Product), **zero integration tests**, **no production-safe migration** (only dev drop/recreate), and the product listing query silently returns $0 price / 0 stock because a computed property never loads its data source.

**Production Readiness Score: 6.5/10**

---

## 1. CRITICAL Issues (Blockers)

| # | Issue | Source(File) | Evaluator |
|---|---|---|---|
| **C1** | `ProductConfiguration.cs:37` maps `ShippingDimensions` as `OwnsOne` but `Product.cs` has **no such property** — will crash at runtime on any Product query | `ProductConfiguration.cs` / `Product.cs` | Architect 3 |
| **C2** | **No EF Core migration** for Catalog tables. Dev uses `EnsureDeleted/EnsureCreated()`. No version-controlled, rollback-able schema. | `Program.cs:64-73` | Architect 3 |
| **C3** | `Product.SalePrice` (l.57) iterates `_variants` with `Where(v => v.SalePrice > 0)`. Product listing queries **never load variants**, so `SalePrice` and `StockQuantity` are always **$0 and 0** in listings. | `Product.cs:57-58` / `GetProductsListQueryHandler.cs` | Domain Expert 1, Architect 2 |
| **C4** | `ProductRelation.UpdateSortOrder()` is **`public`** not `internal` — breaks aggregate encapsulation. | `ProductRelation.cs:38-41` | Architect 1 |
| **C5** | **Zero Catalog integration tests** — Auth has 14 integration tests, Catalog has none, despite full Testcontainers infrastructure. | `tests/AE.Market.Integration.Tests/` | Architect 3 |

---

## 2. HIGH Severity Gaps

### Domain / Business

| # | Gap | Detail | Source |
|---|---|---|---|
| H1 | **No bundle product model** — `ProductType.Bundle` is an enum value but `BundleComponent` entity doesn't exist. Can't define bundle contents. | `ProductType.cs` | Domain Expert 1 |
| H2 | **No configurable product super-attribute model** — Can't link variant-defining attributes (e.g., Color, Size) to the Configurable product type. | `ProductType.cs` | Domain Expert 1 |
| H3 | **Simple/Digital products created without a variant** — `Product.Create()` doesn't enforce an initial variant. A Simple product with 0 variants has $0 price / 0 stock — invalid state. | `Product.cs:81-94` | Domain Expert 1 |
| H4 | **Single-category assignment only** — Product has one `CategoryId`. Can't assign a product to multiple categories. | `Product.cs:38` | Domain Expert 3 |
| H5 | **No tiered/volume pricing** — Only single `SalePrice` + `ListPrice`. No quantity breaks, no customer-group pricing, no time-limited sales. | `ProductVariant.cs:16-17` | Domain Expert 2 |
| H6 | **No multi-currency** — All prices are `decimal` with no currency context. | `ProductVariant.cs:16-17` | Domain Expert 2 |
| H7 | **No warehouse/location-level inventory** — Single scalar `StockQuantity` per variant. No multi-warehouse support. | `ProductVariant.cs:18-20` | Domain Expert 2 |
| H8 | **No tax rate tables or jurisdiction mapping** — `ProductTaxCode` is just a classification label. No rates, no jurisdiction mapping, no tax-inclusive/exclusive flag. | `ProductTaxCode.cs` | Domain Expert 2 |
| H9 | **Attribute groups not persisted** — `AttributeGroup` entity defined with full lifecycle but has **no EF configuration, no DbSet, no table**. Data lost on every request. | `AttributeGroup.cs` / `Category.cs:30-31` | Domain Expert 3, Architect 3 |
| H10 | **No materialized path for category hierarchy** — Adjacency list only. `GetEffectiveAttributes()` and `IsDescendantOf()` load entire ancestor chains into memory. | `Category.cs:127-137,146-156` | Domain Expert 3 |

### CQRS / Application

| # | Gap | Detail | Source |
|---|---|---|---|
| H11 | **Pagination params inaccessible from controllers** — `GetProductsListQuery` accepts `Page`, `PageSize`, `SortBy` etc. but controllers pass zero query string params. All list queries return page 1 / size 20 with no filtering. | Controllers | Architect 2 |
| H12 | **8 reference data queries have no caching** — `GetCategoriesListQuery`, `GetBrandsListQuery`, etc. don't implement `ICachedQuery` despite `CacheKeys` defining keys. | Queries/Categories/, Brands/, etc. | Architect 2 |
| H13 | **`ReserveVariantStock` / `ReleaseVariantStock` don't raise domain events** — Stock reservations are invisible to the outbox system. No cache invalidation, no audit. | `Product.cs:322-340` | Architect 1, Architect 2 |
| H14 | **`ListPrice` is dead code** — Never persisted (no EF mapping), never exposed in `VariantDto`, never accepted by `UpdateVariantPricingCommand`. | `ProductVariant.cs:17,123-127` / `VariantDto.cs` / `UpdateVariantPricingCommand.cs` | Domain Expert 2 |

### Infrastructure / Production

| # | Gap | Detail | Source |
|---|---|---|---|
| H15 | **No global soft-delete query filter** — `BaseEntity.IsDeleted` exists but no `HasQueryFilter(e => !e.IsDeleted)` on any Catalog entity. Soft-deleted items are returned in queries. | All config files | Architect 3 |
| H16 | **No CI/CD pipeline** — `.github/workflows/` is empty. | `.github/workflows/` | Architect 3 |

---

## 3. MEDIUM Severity Issues

| # | Issue | Source | Evaluator |
|---|---|---|---|
| M1 | `UpdateProductCommand` bundles 12+ fields — triggers multiple domain events per call. Should decompose. | `UpdateProductCommand.cs` | Architect 2 |
| M2 | Cache invalidation via outbox has 100s window — stale reads between mutation and eviction. | `DependencyInjection.cs:87` | Architect 2 |
| M3 | `Category.ChangeParent()` throws `DomainException` instead of returning `Result` — violates project convention. | `Category.cs:112-113` | Architect 1 |
| M4 | Event handlers fetch Product entity just for cache invalidation — unnecessary DB round-trip in outbox path. | `ProductDeletedEventHandler.cs`, `ProductUpdatedEventHandler.cs` | Architect 3 |
| M5 | Event naming inconsistency — mix of `ProductCreatedEvent.cs` and `ProductActivatedDomainEvent.cs`. | Events folder | Architect 1 |
| M6 | `GroupUnit` as standalone aggregate root is questionable — has no references from any other aggregate. Pure reference data. | `GroupUnit.cs:8` | Architect 1 |
| M7 | `ReleaseStock` silently clamps to 0 instead of throwing — masks logic errors. | `ProductVariant.cs:152-156` | Domain Expert 2 |
| M8 | No per-variant backorder control — `AllowBackOrder` is on Product only. | `Product.cs:29-30` | Domain Expert 1 |
| M9 | No low-stock thresholds — only stockout alert is a log warning at quantity <= 0. | `ProductVariant.cs` | Domain Expert 2 |
| M10 | No brand-to-category associations — can't determine which brands are in which categories. | `Brand.cs` | Domain Expert 3 |
| M11 | Attribute inheritance is rigid — no override support, no database-level query support. | `Category.cs:139-169` | Domain Expert 3 |
| M12 | `AttributeGroup.Delete()` is `public` while all other methods are `internal`. | `AttributeGroup.cs:51` | Architect 1 |
| M13 | Missing `RemoveUnitFromGroup` command — unit can be added but not removed via Application layer. | Commands folder | Architect 2 |
| M14 | No foreign key existence validation in `CreateProductCommandHandler` — broken FK references possible. | `CreateProductCommandHandler.cs:18-33` | Architect 2 |
| M15 | `CachingBehavior` caches failure `Result<T>` objects — transient DB error could poison cache. | `CachingBehavior.cs:20-26` | Architect 2 |
| M16 | Race condition window between load & save in stock operations — no `DbUpdateConcurrencyException` retry. | `ReserveVariantStockCommandHandler.cs:19-36` | Domain Expert 2 |
| M17 | `FormulaType` enum with hardcoded switch — only Celsius/Fahrenheit. Not extensible without code changes. | `FormulaType.cs` / `GroupUnit.cs:75-93` | Domain Expert 2 |

---

## 4. PROs Summary

| Category | Notable Strengths |
|---|---|
| **Domain Model** | ✅ 55 files, 53 domain events — rich coverage |
| **Value Objects** | ✅ `Slug`, `Sku`, `URL` as immutable records with validation |
| **Encapsulation** | ✅ All collections exposed as `IReadOnlyCollection<T>`, private setters everywhere |
| **Child Entities** | ✅ `internal` constructors/mutators — access only through aggregate root |
| **Invariants** | ✅ Circular parent prevention (`Category`), last-variant guard (`Product`), self-relation prevention, backorder-limit coupling |
| **Stock Operations** | ✅ Reserve/release/adjust pattern with `RowVersion` optimistic concurrency |
| **EAV Attribute System** | ✅ Input type validation, option lists with sort order, category attribute inheritance |
| **Domain Events** | ✅ Raised inside domain layer, rich context (old/new values), consumed via outbox |
| **CQRS** | ✅ 23 command groups, 18 query handlers, all return `Result<T>` |
| **Specification Pattern** | ✅ 8 spec classes with proper includes, filtering, pagination |
| **FluentValidation** | ✅ Validators for every command |
| **Mapster Mappings** | ✅ All Catalog DTOs configured |
| **Cache Infrastructure** | ✅ FusionCache + Redis, `ICachedQuery` marker, `CacheKeys` defined for all entities |
| **API Layer** | ✅ 5 REST controllers, proper `[Authorize]` + `[HasPermission]` on mutations, public GET endpoints |
| **EF Configurations** | ✅ 14 configs, proper `HasConversion` for VOs, `"catalog"` schema, cascade behaviors |
| **Seeders** | ✅ 5 seeders including 60+ tax codes (Avalara-compatible `txcd_*` scheme) |
| **Architecture Tests** | ✅ Layer dependency tests, convention tests, feature structure tests |
| **Domain Tests** | ✅ 20 test files covering all entities |
| **Outbox Pattern** | ✅ `DomainEventDispatcher` + `OutboxProcessorJob` with `FOR UPDATE SKIP LOCKED` |
| **Result Pattern** | ✅ Custom `Result<T>` throughout, no FluentResults dependency, proper HTTP status mapping |
| **Cross-Aggregate References** | ✅ Aggregate roots reference each other by ID, not object reference |

---

## 5. Top Recommendations (Priority Order)

1. **[C1/C2/C3]** Fix the 3 critical blockers — `ShippingDimensions` in `ProductConfiguration.cs`, add EF migration, fix `SalePrice` returning $0 in listings.
2. **[H9]** Persist `AttributeGroup` — create EF config + DbSet, or migrate to `OwnsMany` value objects.
3. **[H13/H14]** Add missing `ListPrice` mapping + DTO + command, add domain events for reserve/release stock.
4. **[H11]** Wire pagination/sort/filter parameters from controllers to queries.
5. **[H12]** Implement `ICachedQuery` on all 8 reference data queries.
6. **[H1/H2]** Build Bundle components and Configurable super-attribute models.
7. **[C5/H16]** Add Catalog integration tests + CI pipeline.
8. **[H10]** Add materialized path to category hierarchy.
9. **[C4]** Fix `ProductRelation.UpdateSortOrder()` → `internal`.
10. **[H4/M10]** Implement many-to-many product-to-category + brand-to-category associations.
