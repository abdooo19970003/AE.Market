# Software Architecture Evaluation: CQRS Patterns

## Executive Summary

The Catalog aggregate implements a solid CQRS foundation with clean separation of commands and queries, consistent patterns across 23 command groups and 17 query handlers, and proper use of MediatR pipeline behaviors. The specification pattern, domain events via outbox, and cache invalidation through event handlers demonstrate thoughtful architecture. However, there are notable gaps: pagination parameters are entirely inaccessible from the API controllers (making list queries effectively useless), cache markers are missing on all reference data queries (categories, brands, group units, tax codes), event-driven cache invalidation introduces inconsistency for stock operations, the `UpdateProduct` command is overly coarse, and the `ReserveVariantStock`/`ReleaseVariantStock` operations lack domain events entirely.

---

## PROS

- **Consistent CQRS triad pattern**: Every operation follows `Command/Query record + Handler + Validator` under a dedicated folder, with proper `ICommand<T>`/`IBaseQuery<T>` interfaces. (e.g., `CreateProductCommand`, `CreateProductCommandHandler`, `CreateProductCommandValidator`)

- **Clean query specialization**: Instead of one bloated "get product" query, there are 5 specialized queries: by ID, by slug, by brand (paginated), by category (paginated), and list (paginated). Each serves a distinct use case.

- **Specification pattern properly applied**: Data access is abstracted behind `IReadRepository<T>` for queries and `IRepository<T>` for commands. Specifications like `ProductByIdSpec` encapsulate filtering, includes, pagination, and sorting.

- **Pipeline behaviors correctly scoped**: `TransactionBehavior` is constrained to `IBaseCommand` (lines 13 of `TransactionBehavior.cs`), ensuring only mutations are transactional. `CachingBehavior` targets `ICachedQuery` (line 12 of `CachingBehavior.cs`), keeping it query-only.

- **Domain events via outbox (async boundary)**: All domain events (e.g., `ProductCreatedDomainEvent`, `VariantStockAdjustedDomainEvent`) are raised by entities, captured by `DomainEventDispatcher` interceptor, written to the `outbox_messages` table, and processed asynchronously by `OutboxProcessorJob`. This preserves the transactional boundary and avoids in-process side effects.

- **Event-driven cache invalidation**: Handlers like `ProductCreatedEventHandler`, `ProductUpdatedEventHandler`, and `VariantStockAdjustedEventHandler` properly evict affected cache keys on domain events, keeping the cache coherent with the write model.

- **Controllers properly separate read/write concerns**: Query endpoints (`GET`) have no auth attributes; command endpoints (`POST/PUT/DELETE/PATCH`) require `[Authorize]` and `[HasPermission(...)]`, enforcing the CQS principle at the API layer.

- **Well-scoped CacheKeys**: Each cacheable entity has a dedicated static methods class (`CacheKeys.cs`) with consistent naming conventions (e.g., `ProductById(Guid id)` returns `$"product-{id}"`).

- **Consistent Result pattern across all handlers**: Every handler returns `Result<T>` or `Result`, never throws for control flow. Domain exceptions are caught and mapped to `Result.Fail()`.

- **Good command granularity for variants**: Stock operations are correctly split into separate commands (`UpdateVariantStock`, `AdjustVariantStock`, `ReserveVariantStock`, `ReleaseVariantStock`, `UpdateVariantPricing`) instead of a single monolithic variant update.

---

## GAPS (Missing or Incomplete)

| Severity | Gap | File(s) | Description |
|----------|-----|---------|-------------|
| **Critical** | **Pagination parameters inaccessible from controllers** | `ProductsController.cs` lines 26-58; `GetProductsListQuery.cs`; `GetProductsByBrandQuery.cs`; `GetProductsByCategoryQuery.cs` | The list query records accept `Page`, `PageSize`, `SortBy`, `SortDescending`, `IsActive`, and `Search` parameters, but the controller endpoints `GetProducts()`, `GetProductsByBrand(Guid brandId)`, and `GetProductsByCategory(Guid categoryId)` accept **zero** query string parameters. All list queries return page 1 with page size 20 with no sorting and no filtering. This effectively breaks pagination and filtering for the entire product catalog API. |
| **High** | **Reference data queries have no caching** | `GetCategoriesListQuery.cs`, `GetBrandsListQuery.cs`, `GetGroupUnitsListQuery.cs`, `GetProductTaxCodesListQuery.cs`, `GetCategoryByIdQuery.cs`, `GetBrandByIdQuery.cs`, `GetGroupUnitByIdQuery.cs`, `GetProductTaxCodeByIdQuery.cs` | None of these 8 queries implement `ICachedQuery`. Despite `CacheKeys` defining keys for all of them (e.g., `categories-list`, `brands-list`), the caching behavior never fires for these frequently accessed, rarely changing reference data lookups. Every call hits the database. |
| **Low** | **Missing `RemoveUnitFromGroup` command** | Events: `GroupUnitUnitRemoved`; Commands folder | The `GroupUnitUnitRemoved` domain event exists in the Events folder, but there is no corresponding `RemoveUnitFromGroupCommand` in the Commands folder and no API endpoint. Units can be added to groups but never removed. |
| **Low** | **Missing commands for category attribute management** | Events: `CategoryAttributeAdded`, `CategoryAttributeRemoved`; `Category.cs` domain entity | Domain events exist for adding/removing category attributes, but there are no application commands or handlers to trigger them. |
| **Low** | **Missing Activate/Deactivate commands** | Events: `BrandActivated`, `BrandDeactivated`, `CategoryActivated`, `CategoryDeactivated` | Activation/deactivation is handled implicitly via `Delete()` and `Restore()` on entities, but the domain events are still fired. There are no explicit `ActivateBrand`/`DeactivateBrand` commands. |
| **Low** | **Category parent cannot be changed after creation** | `UpdateCategoryCommand.cs` line 6-12; Event: `CategoryParentChanged` | `CreateCategoryCommand` accepts `ParentId` but `UpdateCategoryCommand` does not. However, the domain entity fires `CategoryParentChangedDomainEvent`, suggesting the domain supports it. |
| **Low** | **Brand slug cannot be changed after creation** | `UpdateBrandCommand.cs` line 6-14; Event: `BrandSlugChanged` | `CreateBrandCommand` accepts `Slug` but `UpdateBrandCommand` does not. The domain entity fires `BrandSlugChangedDomainEvent`. |

---

## ISSUES (Design/Implementation Problems)

| Severity | Issue | File(s) | Description |
|----------|-------|---------|-------------|
| **High** | **`ReserveVariantStock` and `ReleaseVariantStock` do not raise domain events** | `Product.cs` lines 322-340 | `ReserveVariantStock()` and `ReleaseVariantStock()` call `UpdateLastModified()` but **never call `AddDomainEvent()`**. This means stock reservation and release operations are invisible to the outbox system. No cache invalidation happens, no audit trail exists, and no downstream consumers are notified. Compare with `AdjustVariantStock()` (line 309-320) which correctly raises `VariantStockAdjustedDomainEvent`. |
| **Medium** | **`UpdateProduct` command is too coarse** | `UpdateProductCommand.cs`; `UpdateProductCommandHandler.cs` | Single command bundles name, slug, details, category, brand, tax code, product type, backorders, short description, long description, meta title, meta description, and meta keywords. This violates the CQRS principle of focused commands. Any partial update requires the client to send all fields. It also triggers **multiple domain events** per update (`ProductDetailsUpdated`, `ProductTypeChanged`, `ProductSlugChanged`, `ProductMetaFieldsUpdated`, etc.) even when only one field changed. Better decomposition: `UpdateProductDetails`, `ChangeProductCategory`, `UpdateProductSlug`, `UpdateProductMeta`, `SetProductBackOrder`, etc. |
| **Medium** | **Cache invalidation is inconsistent across mutation paths** | `ProductCreatedEventHandler.cs`; `ProductUpdatedEventHandler.cs`; `VariantStockAdjustedEventHandler.cs` | Cache eviction happens only through domain event handlers, which run **asynchronously via the outbox** (polled every 100s per AGENTS.md). This means after a mutation: (1) the stale product-by-id or product-list cache is served for up to 100 seconds (stale read), (2) the new data is already committed to the database but the cache is not updated, (3) there is no write-through or write-behind caching strategy. This is a deliberate tradeoff for eventual consistency, but with a 100s outbox interval, the inconsistency window is large. |
| **Medium** | **`CachingBehavior` will cache failure results** | `CachingBehavior.cs` lines 20-26 | The behavior wraps `next(cancellationToken)` inside `cache.GetOrCreateAsync()`. If the underlying handler returns a `Result.Fail(...)` (e.g., "product not found"), that failure `Result<T>` object gets cached. Subsequent requests for the same key will receive the cached failure without re-executing the query. A transient failure (e.g., database timeout) could poison the cache for the absolute expiration duration. |
| **Medium** | **No foreign key existence validation in commands** | `CreateProductCommandHandler.cs` lines 18-33 | `CreateProductCommandHandler` receives `BrandId`, `CategoryId`, and `TaxCodeId` but does not verify they reference existing entities. If a client sends a non-existent ID, the product is created with a broken foreign key reference. The `UpdateProductCommandHandler` has the same issue. |
| **Medium** | **`GetProductsListQueryHandler` creates specs inline instead of using dedicated spec class** | `GetProductsListQueryHandler.cs` lines 18-32 | All other list queries use dedicated spec classes (e.g., `ProductsByBrandSpec`, `ProductsByCategorySpec`), but `GetProductsListQueryHandler` creates a `BaseSpecification<Product>` inline. This duplicates the sort-by logic already present in `ProductsByBrandSpec` and `ProductsByCategorySpec`. Inconsistency: if sort logic changes, it must be updated in 3 places. |
| **Low** | **Potential N+1 in product detail queries** | `GetProductByIdQueryHandler.cs` lines 29-44; `GetProductBySlugQueryHandler.cs` lines 29-44 | When `IncludeChildren = true`, the handler first loads the product with includes (1 query), then issues **2 additional queries** to load brand and category by ID (2 more queries). This could be optimized by including Brand/Category navigation properties in the specification. |
| **Low** | **`LoggingBehavior` has wrong generic constraint** | `LoggingBehavior.cs` line 12 | Constraint `where TResponse : Result` restricts this behavior to handlers returning `Result`. While all current handlers return `Result`, this constraint is overly restrictive and would silently skip logging for any future non-Result handler. Should be `where TRequest : IRequest<TResponse>` without constraining `TResponse`. |
| **Low** | **`Enum.Parse` in handlers bypasses validator safety net** | `CreateProductCommandHandler.cs` line 17; `UpdateProductCommandHandler.cs` line 22 | Both handlers use `Enum.Parse(typeof(ProductType), request.ProductType)` which throws `ArgumentException` on invalid input. While the validator does check `IsEnumName`, if validation is skipped (e.g., in tests), this throws a non-domain exception. Consider using `Enum.TryParse` with a fallback error. |
| **Low** | **Duplicate sort-by logic across spec classes** | `ProductsByBrandSpec.cs` lines 18-28; `ProductsByCategorySpec.cs` lines 18-28; `GetProductsListQueryHandler.cs` lines 22-31 | The same `sortBy.ToLowerInvariant() switch` block for mapping sort field names to expressions is duplicated in 3 places. Should be extracted to a shared helper or the spec base class. |
| **Low** | **`TransactionBehavior` uses reflection for dynamic Result construction** | `TransactionBehavior.cs` lines 38-50 | Uses `MakeGenericType`, `GetMethods`, and `Invoke` to construct `Result<T>.Fail(...)`. This is fragile, slow, and bypasses compile-time safety. A `Result.Fail<T>(Error)` static method on the non-generic `Result` class would eliminate the need for reflection. |
| **Low** | **`AddUnitToGroup` handler doesn't call `repo.Update(groupUnit)`** | `AddUnitToGroupCommandHandler.cs` line 33 | The handler calls `groupUnit.AddUnit(...)` which adds to the in-memory collection but never calls `repo.Update(groupUnit)` or marks the entity as modified. This relies on the `TransactionBehavior` detecting changes via `SaveChangesAsync`, which works because EF Core tracks the entity (loaded with tracking via `GetByIdWithTrackingAsync`). However, it's inconsistent with other mutation handlers that explicitly call `repo.Update(...)`. |

---

## Recommendations (Top 5 CQRS Improvements)

### 1. Decompose `UpdateProduct` into Focused Commands (High Priority)

**Problem**: `UpdateProductCommand` is a monolith that bundles 12+ fields, causing unnecessary domain events and forcing clients to send complete snapshots.

**Solution**: Split into focused commands:
- `UpdateProductDetailsCommand` (name, short/long description)
- `ChangeProductCategoryCommand` (categoryId)
- `ChangeProductBrandCommand` (brandId)
- `UpdateProductTaxCodeCommand` (taxCodeId)
- `UpdateProductSlugCommand` (slug)
- `UpdateProductMetaCommand` (meta title/description/keywords)
- `SetProductBackOrderCommand` (allowBackOrder, backOrderLimit)

Each command handler calls only the relevant domain method(s), raising only the necessary domain event(s).

### 2. Implement ICachedQuery on All Reference Data Queries (Medium Priority)

**Problem**: 8 reference data queries (categories, brands, group units, tax codes) have no caching despite CacheKeys defining keys for them.

**Solution**: Implement `ICachedQuery` on:
- `GetCategoriesListQuery`, `GetCategoryByIdQuery`
- `GetBrandsListQuery`, `GetBrandByIdQuery`
- `GetGroupUnitsListQuery`, `GetGroupUnitByIdQuery`
- `GetProductTaxCodesListQuery`, `GetProductTaxCodeByIdQuery`

Use shorter absolute expirations (5-15 minutes) for lists and longer ones (30-60 minutes) for single entities.

### 3. Expose Pagination/Sorting/Filtering Parameters in Controllers (High Priority)

**Problem**: `GetProductsListQuery`, `GetProductsByBrandQuery`, and `GetProductsByCategoryQuery` accept pagination params, but the controller endpoints ignore them entirely.

**Solution**: Update controller endpoints:
```csharp
// ProductsController.cs
[HttpGet]
public async Task<IActionResult> GetProducts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] bool? isActive = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] bool sortDescending = false,
    CancellationToken ct)
{
    var result = await mediator.Send(
        new GetProductsListQuery(page, pageSize, isActive, sortBy, sortDescending), ct);
    return result.ToActionResult();
}
```

Apply the same pattern to `brand/{brandId}` and `category/{categoryId}` endpoints.

### 4. Add Domain Events to ReserveVariantStock and ReleaseVariantStock (High Priority)

**Problem**: Stock reservation and release are invisible to the event system -- no `AddDomainEvent()` call in `ReserveVariantStock()` or `ReleaseVariantStock()`.

**Solution**: In `Product.cs`, add:
```csharp
public void ReserveVariantStock(Guid variantId, int quantity)
{
    // ... existing logic ...
    variant.ReserveStock(quantity);
    AddDomainEvent(new VariantStockReservedDomainEvent(Id, variantId, quantity));
    UpdateLastModified();
}

public void ReleaseVariantStock(Guid variantId, int quantity)
{
    // ... existing logic ...
    variant.ReleaseStock(quantity);
    AddDomainEvent(new VariantStockReleasedDomainEvent(Id, variantId, quantity));
    UpdateLastModified();
}
```

Create corresponding `VariantStockReservedDomainEvent` and `VariantStockReleasedDomainEvent` classes and event handlers to evict affected cache keys.

### 5. Add RemoveUnitFromGroup Command and Validate Foreign Keys on Create (Medium Priority)

**Problem**: Missing `RemoveUnitFromGroup` command leaves the unit-group model incomplete. Additionally, `CreateProductCommandHandler` does not validate that `BrandId`/`CategoryId`/`TaxCodeId` exist.

**Solution**:
1. Create `RemoveUnitFromGroupCommand(GroupUnitId, UnitId)` with handler and validator.
2. In `CreateProductCommandHandler` (and `UpdateProductCommandHandler`), validate foreign keys:
```csharp
var brandExists = await brandRepo.AnyAsync(new BrandByIdSpec(request.BrandId), ct);
if (!brandExists) return Result<ProductDto>.Fail(CatalogErrors.BrandNotFound);
var categoryExists = await categoryRepo.AnyAsync(new CategoryByIdSpec(request.CategoryId), ct);
if (!categoryExists) return Result<ProductDto>.Fail(CatalogErrors.CategoryNotFound);
```

This aligns with the existing pattern used in query handlers that load related entities only when needed.
