# Software Architect Review — Clean Architecture & CQRS

**Reviewer:** Omar — CQRS, .NET backend systems, MediatR pipelines, EF Core

---

## 1. Domain Model Quality and Aggregate Boundaries

The Catalog domain is well-structured overall, with `Product` and `Category` as distinct aggregate roots (both implement `IAggregateRoot`). The choice to nest `ProductVariant`, `AttributeValue`, and `ProductImage` as child entities of `Product` is sound — a variant has no meaning outside its product, and enforcing access only through the `Product.AddVariant()` factory preserves aggregate consistency. Similarly, `CategoryAttribute` and `AttributeOption` are correctly encapsulated as children of `Category`, and the `Category.ChangeParent()` guard against self-referencing is a solid invariant.

However, there are two notable design inconsistencies. First, **`Product.Slug` and `ProductVariant.Sku` are raw `string` types**, even though `Slug` and `Sku` value objects already exist in the domain (`Domain/Aggregates/Catalog/ValueObjects/`). The domain already defines `Sku.Create()` with validation (3-50 chars, uppercase, digits, hyphens) and `Slug.Create()` with normalization logic, so the entities should use these value objects directly (e.g., `public Slug Slug { get; private set; }`). The raw string approach pushes validation into the Application layer and weakens the domain's self-defending nature.

Second, **`Category.ChangeParent()` throws a `DomainException` directly**, which violates the project's stated convention that "Handlers return `Result<T>` or `Result`, never throw for control flow." This self-parent check should either return a boolean/failure indicator, or better, be validated in the command handler before calling the domain method, leaving the domain method to assume valid input. The existing `TransactionBehavior` and `ExceptionHandlerBehavior` can catch the exception, but that routes domain validation through the exception path rather than the intentional `Result` flow, which is the pattern used everywhere else in the codebase (e.g., `AuthErrors.EmailAlreadyExists`).

## 2. Application Layer Stubs and Pattern Compatibility

The Application stubs follow the established folder convention correctly (`Commands/`, `Queries/`, `DTOs/`, `Specs/`, `CacheKeys/`, `Events/`), and the architecture tests (`FeatureStructureTests`) confirm this structure passes. The event handler stubs are correctly wired via `DomainEventNotification<T>` and will work with the existing `DomainEventDispatcher` interceptor + `OutboxProcessorJob` pipeline.

The existing MediatR pipeline behaviors will work well for Catalog operations with no changes needed:

- **`TransactionBehavior`** constrains itself to `IBaseCommand`, so all Catalog commands (which implement `ICommand<TResponse>` extending `IBaseCommand`) will automatically participate in the ambient transaction, including the `SaveChangesAsync` + commit flow. This is correct.
- **`CachingBehavior`** constrains itself to `ICachedQuery` — Catalog queries that implement `ICachedQuery` (e.g., `GetProductByIdQuery`, `GetCategoryTreeQuery`) will be auto-cached via FusionCache. The marker requires `CacheKey`, `AbsoluteExpiration`, and `SlidingExpiration`, so production Catalog queries must provide these. The architecture test (`CachedQueries_Should_Have_AbsoluteExpiration_Always`) already enforces this.
- **`ValidationBehavior`** will validate all Catalog commands that have FluentValidation validators. The architecture test (`EveryCommand_Should_HaveCorrespondingValidator`) enforces that every command has a validator, so Catalog commands cannot skip validation.

The **empty `CacheKeys` class** is a red flag — it must be populated with actual cache key helpers (e.g., `internal static string ProductById(Guid id) => $"catalog:product:{id}"`) before any caching strategy can work. The existing Auth pattern (`CacheKeys.UserId(Guid id)`) is the template to follow. Cache invalidation is another gap: when a `UpdateProductCommand` commits, the corresponding cache entries (product-by-id, product-list, category-by-id) must be evicted. The cleanest approach here would be for the Catalog event handlers (which are currently stubs returning `Task.CompletedTask`) to call `ICacheService.RemoveAsync(...)` with the affected keys. This keeps cache invalidation inside event handlers rather than coupling it into command handlers.

## 3. EF Core, Specifications, and Data Access Readiness

**Nothing exists yet in the Infrastructure layer for Catalog.** The `AppDbContext` only declares `DbSet<>` properties for Auth entities, and the `Persistence/Configurations/` directory has no Catalog configurations. This must be addressed before any Catalog operation works. My specific recommendations:

### Add EF Configurations

- **Add `DbSet<Product>`, `DbSet<Category>`, `DbSet<ProductVariant>`, `DbSet<CategoryAttribute>`** to `AppDbContext`. Because `ApplyConfigurationsFromAssembly` auto-discovers all `IEntityTypeConfiguration<T>` files, simply adding the configuration classes in `Configurations/Catalog/` will wire everything up.
- **Key configuration concerns:**
  - `ProductVariant` needs an owned/collection configuration with `AttributeValue` (the polymorphic value columns `ValueText`, `ValueInteger`, `ValueDecimal`, `ValueOptionId`, `ValueBoolean`, `ValueDateTime` map cleanly to separate nullable columns, but consider a `Jsonb` column for the EAV-style flexibility in the long run).
  - `Category` self-referencing (`ParentId` → `Id`) needs a proper foreign key with cascade behavior (likely `Restrict` to avoid multiple cascade paths).
  - **Soft delete filtering:** `BaseEntity.IsDeleted` is public, but the current `Repository<T>` does not apply a global query filter. Either add `modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted)` to each configuration, or configure it generically in `OnModelCreating` via `GetApplicationEntities()`. Without this, queries will return soft-deleted Catalog items.

### Specifications

Specifications should mirror the Auth pattern: each query gets its own spec class inheriting `BaseSpecification<T>` with expressive `AddInclude()` chains for eager-loading (e.g., `ProductWithVariantsSpec` includes `Variants`, `Variants.AttributeValues`, `Images`). The `Placeholder.cs` in `Specs/` must be replaced with real spec classes.

### Mapster

`IMapper` / Mapster configuration for Catalog DTOs (e.g., `ProductDto`, `CategoryTreeItemDto`) must be added to `MappingConfig.cs`. The existing config uses `TypeAdapterConfig.GlobalSettings`, so simply add `config.NewConfig<Product, ProductDto>()` entries.

## 4. Aggregate Isolation, Coupling, and Architecture Test Coverage

The `FeatureAggregateIsolationTests` are correctly configured: the Catalog features (`Categories`, `Products`, `Variants`, `Attributes`) are mapped to `AE.Market.Domain.Aggregates.Catalog`, meaning Application feature code can freely reference all domain types within the Catalog namespace. This is appropriate since these entities form a single logical bounded context — a Product is meaningless without its Category, and Variants/Attributes derive from Category-level attribute definitions. The existing `Category.ChangeParent()` self-referencing guard and `Category.AddAttribute()`/`Product.AddVariant()` factory methods maintain aggregate boundaries at the domain level.

A critical concern: **Product holds a `CategoryId` (weak reference) but has no `Category` navigation property** — this is correct DDD practice for cross-aggregate references. However, the application layer must validate that `CategoryId` exists before accepting a Product create/update. The existing Auth pattern demonstrates this: `RegisterCommandHandler` calls `repo.AnyAsync(new UserByEmailSpec(email))` before proceeding. Catalog command handlers should similarly call `repo.AnyAsync(new CategoryByIdSpec(categoryId))` and return `CatalogErrors.CategoryNotFound` on failure.

**The `Queries_Should_NotInjectWritRepository` architecture test** will enforce that Catalog query handlers inject `IReadRepository<T>` (not `IRepository<T>`), which is already correct by design. But this also means Catalog queries that need to read Product with its Variants must use specifications with `.AddInclude()`, because `IReadRepository` only exposes read methods via the specification evaluator which supports includes.

## 5. Production-Ready Action Items

To make the Catalog feature production-ready, the following are the highest-priority changes:

1. **Add EF Core configurations** in `Infrastructure/Persistence/Configurations/Catalog/` for `ProductConfiguration`, `CategoryConfiguration`, `ProductVariantConfiguration`, `CategoryAttributeConfiguration`, etc. Register `DbSet<>` entries in `AppDbContext`. Add a global soft-delete query filter.
2. **Replace the four Placeholder files** in `Application/Features/Catalog/` with real CQRS implementations: at minimum `CreateCategoryCommand`, `GetProductByIdQuery`, `UpdateProductCommand`, `ListProductsQuery` with their handlers, validators, and DTOs. Follow the Auth pattern exactly (sealed records, ICommand/IBaseQuery marker interfaces, specification-driven data access).
3. **Populate `CacheKeys`** with key helpers and implement cache eviction in the event handlers (the stubs in `Events/`). The `ProductUpdatedEventHandler` and `CategoryUpdatedEventHandler` should call `ICacheService.RemoveAsync()` for all affected keys.
4. **Add Mapster configurations** for Catalog DTOs in `MappingConfig.cs` before the `.Compile()` call.
5. **Refactor Value Object usage**: Change `Product.Slug` from `string` to `Slug`, and `ProductVariant.Sku` from `string` to `Sku`. The existing `ProductVariant.Create(Guid id, Guid productId, string name, string sku)` signature accepts raw `string`, but it should accept `Sku` and have the caller (domain factory or application handler) create the value object first.
6. **Add API controllers** (`CategoriesController`, `ProductsController`) following the established pattern: `[Route("api/[controller]")]`, sealed class, primary constructor with `IMediator`, `result.ToActionResult()` pipeline.
7. **Refactor `Category.ChangeParent()`** to either not throw (use a `bool` return or guard in the application handler) to stay consistent with the Result pattern convention.
