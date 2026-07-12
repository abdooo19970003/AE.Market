# Software Architecture Evaluation: Clean Architecture / Layering

## Executive Summary

The Catalog aggregate is **surprisingly well-implemented** for a codebase explicitly documented as "only Auth has code." The domain model is rich, the CQRS structure is complete, the API controllers exist and are properly secured, and the Infrastructure layer has all 14 EF configurations. However, there is a **critical production gap**: the EF Core migrations (`InitialCreate`) pre-date the Catalog entities and only create `auth` and `outbox` schemas -- the `catalog` tables exist only via `EnsureDeleted/EnsureCreated` in Development mode. This means **no production-safe migration path** exists for Catalog. Additionally, there are **zero integration tests** for Catalog and **no global soft-delete query filter** is applied. The architecture is sound overall, but the project needs a focused effort on production hardening before deployment.

---

## PROS

### Domain Layer (Excellent)
- **Rich domain model with proper encapsulation**: `Product`, `Category`, `Brand`, `GroupUnit`, `ProductTaxCode` are all sealed classes with `IAggregateRoot`, private constructors, and factory methods (`Create()`). File: `Domain/Aggregates/Catalog/Products/Product.cs`, `Category.cs`
- **Value objects properly implemented**: `Slug`, `Sku`, and `URL` are sealed records with validation logic. `Sku.Create()` validates format via regex. `Slug.Create()` normalizes input. File: `Domain/Aggregates/Catalog/ValueObjects/`
- **53 domain events** defined across all Catalog entities -- every mutation emits an event. Events follow `[Entity][Action]DomainEvent` naming convention. File: `Domain/Aggregates/Catalog/Events/`
- **Domain errors are centralized** as static `Error` records in `CatalogErrors.cs` with consistent `Catalog.*` code prefixes. File: `Domain/Aggregates/Catalog/Errors/CatalogErrors.cs`
- **Invariant enforcement**: `Category.ChangeParent()` prevents self-referencing and circular hierarchies. `Product.RemoveVariant()` prevents removing last variant from active product. `GroupUnit.AddUnit()` prevents duplicate base units. File: various domain entities.
- **Zero external dependencies** in Domain project - pure .NET. File: `Domain/AE.Market.Domain.csproj`

### Application Layer (Very Strong)
- **Full CQRS with 22 command folders and 5 query folders** -- every Catalog operation has a command/query, handler, validator, spec, and DTO. File: `Application/Features/Catalog/Commands/`, `Queries/`
- **FluentValidation validators exist for all commands** (e.g., `CreateBrandCommandValidator` enforces max lengths). File: `Application/Features/Catalog/Commands/CreateBrand/CreateBrandCommandValidator.cs`
- **Mapster configurations complete** for all Catalog DTOs (`BrandDto`, `CategoryDto`, `ProductDto`, `ProductDetailDto`, `VariantDto`, `GroupUnitDto`, `UnitDto`, `ProductTaxCodeDto`). File: `Application/Common/Mapping/MappingConfig.cs`
- **Cache invalidation via domain events**: Event handlers like `ProductUpdatedEventHandler` call `ICacheService.RemoveAsync()` on all affected cache keys. File: `Application/Features/Catalog/Events/ProductUpdated/ProductUpdatedEventHandler.cs`
- **CacheKeys static class** with helper methods for all Catalog entities. File: `Application/Features/Catalog/CacheKeys.cs`
- **Specifications pattern** used for all queries (8 spec classes). File: `Application/Features/Catalog/Specs/`

### API Layer (Good)
- **All 5 Catalog controllers exist**: `CategoriesController`, `BrandsController`, `ProductsController`, `GroupUnitsController`, `ProductTaxCodesController`. File: `AE.Market.API/Controllers/`
- **Proper authentication/authorization**: All mutation endpoints use `[Authorize]` + `[HasPermission(Permission.Mutate*)]`. Public GET endpoints have no auth requirement. File: controller files.
- **RESTful conventions**: Proper HTTP methods (GET, POST, PUT, DELETE, PATCH), proper route patterns (`api/brands`, `api/categories`, `api/products/{id}/variants/{vid}/stock`). File: controller files.
- **Sealed controllers with primary constructors** injecting `IMediator` -- consistent with AGENTS.md convention. File: various controllers.
- **Result pattern consistently used**: All endpoints pipe through `result.ToActionResult()/.ToCreatedActionResult()/.ToNotFoundActionResult()/.ToDeletedActionResult()`. File: `AE.Market.API/Helpers/ResultMapper.cs`

### Infrastructure Layer (Solid Foundations)
- **14 EF Core configurations** all present, using consistent `"catalog"` schema. Configurations use `HasConversion()` for value objects (`Slug`, `Sku`, `URL`), proper `HasMaxLength()`, `HasPrecision()`, and cascade behaviors. File: `AE.Market.Infrastructure/Persistence/Configurations/Catalog/`
- **All DbSets registered** in `AppDbContext`. File: `AE.Market.Infrastructure/Persistence/AppDbContext.cs`
- **Repository pattern correctly abstracts EF Core** -- `IRepository<T>` for writes, `IReadRepository<T>` for reads with `AsNoTracking()`. File: `AE.Market.Infrastructure/Persistence/Repository/Repository.cs`
- **Outbox pattern fully implemented**: `DomainEventDispatcher` interceptor writes to `outbox.outbox_messages` table. `OutboxProcessorJob` uses `FOR UPDATE SKIP LOCKED`. File: `AE.Market.Infrastructure/Persistence/Outbox/OutboxProcessorJob.cs`
- **FusionCache integration** with Redis, fail-safe enabled, stampede protection. File: `AE.Market.Infrastructure/DependencyInjection.cs`

### Architecture Tests (Strong)
- **Layer dependency tests** enforce Clean Architecture rules (Domain does not depend on Application/Infrastructure/API). File: `AE.Market.ArchitectureTests/LayerTests.cs`
- **Convention tests**: Handlers must return `Result`, DTOs must be sealed, controllers must have single IMediator constructor, entities must have private parameterless constructors. Files: `AE.Market.ArchitectureTests/Domain/`, `Application/`, `API/`
- **Feature structure tests** enforces every feature has Commands, Queries, DTOs, Specs folders. File: `AE.Market.ArchitectureTests/Application/FeatureStructureTests.cs`

### Domain Tests (Comprehensive)
- **Catalog domain tests exist for all entities**: `BrandTests.cs` (73 lines), `CategoryTests.cs` (428 lines), `ProductTests.cs` (701 lines), `ProductVariantTests.cs` (386 lines), `GroupUnitTests.cs` (215 lines), `ProductTaxCodeTests.cs` (100 lines), `ProductRelationTests.cs`, `UnitTests.cs`, `ShippingDimensionsTests.cs`. File: `tests/AE.Market.Domain.Tests/Aggregates/Catalog/`

### Seeders (Functional)
- **All Catalog seeders present**: `CategorySeeder`, `BrandSeeder`, `ProductTaxCodeSeeder` (extensive -- 50+ tax codes), `GroupUnitSeeder`. `DbSeeder` orchestrates them. Files: `AE.Market.Infrastructure/Persistence/Seeders/`

---

## GAPS

### Critical
- **No EF Core migration for Catalog tables** -- The `InitialCreate` migration (20260526050407) only creates `auth` and `outbox` schemas. The existing migrations have no catalog schema. The dev workflow uses `EnsureDeleted/EnsureCreated()` (line 69-70 of Program.cs), which recreates the schema from the model. This is **not production-safe** -- schema changes cannot be version-controlled or rolled back. Files: `AE.Market.Infrastructure/Persistence/Migrations/20260526050407_InitialCreate.cs`, `AE.Market.API/Program.cs:64-73`
- **No global soft-delete query filter** -- `BaseEntity` has `IsDeleted` but no `HasQueryFilter(e => !e.IsDeleted)` is applied. Catalog queries will return soft-deleted items. File: `AE.Market.Infrastructure/Persistence/Configurations/Catalog/*.cs` (none have query filters)

### High
- **No integration tests for Catalog** -- `AuthIntegrationTests.cs` has 14 tests covering the full auth flow. There are zero Catalog integration tests. File: `tests/AE.Market.Integration.Tests/AuthIntegrationTests.cs` (no Catalog equivalent)
- **No CI/CD pipeline** -- `BACKEND_PLAN.md` references `.github/workflows/ci.yml` but the directory is empty. No automated build/test/deploy. File: `.github/workflows/` (empty)
- **Dockerfile copies only API project** -- Line 15: `COPY ["AE.Market.API/AE.Market.API.csproj", "AE.Market.API/"]` -- Only copies the API csproj, not the solution file. The `COPY . .` on line 17 copies everything, so it works, but the restore step can't leverage layer caching for the other projects. File: `AE.Market.API/Dockerfile`

### Medium
- **`ProductConfiguration` references `ShippingDimensions` owned entity** (line 38-43) but `Product.cs` does not define a `ShippingDimensions` property. This will cause a runtime `InvalidOperationException` when EF Core tries to configure it. File: `AE.Market.Infrastructure/Persistence/Configurations/Catalog/ProductConfiguration.cs`, `Domain/Aggregates/Catalog/Products/Product.cs`
- **`Category.ChangeParent()` throws `DomainException`** instead of returning a `Result` -- violates the project's own convention that "Handlers return `Result<T>` or `Result`, never throw for control flow." File: `Domain/Aggregates/Catalog/Category.cs:112-113`
- **Event handlers fetch the Product entity just for cache invalidation** -- `ProductDeletedEventHandler` and `ProductUpdatedEventHandler` call `productRepo.GetByIdAsync()` to get `Slug`, `CategoryId`, `BrandId` just for cache key construction. This is an unnecessary DB round-trip in the outbox processing path. File: `Application/Features/Catalog/Events/ProductDeleted/ProductDeletedEventHandler.cs:20`, `ProductUpdatedEventHandler.cs:20`
- **Several domain event names inconsistently use `Event` vs `DomainEvent` suffix** -- e.g., `CategoryCreatedEvent` (line 15) vs `CategoryCreatedDomainEvent` (line 53). The `DomainTests.DomainEvents_Should_HaveDomainEventsPostfix` architecture test enforces the `DomainEvent` suffix, so `CategoryCreatedEvent` is a naming violation. Files: `Domain/Aggregates/Catalog/Events/CategoryCreatedEvent.cs`, `CategoryCreatedDomainEvent.cs`

### Low
- **Missing FK indexes**: `Product.BrandId`, `Product.CategoryId`, `Product.TaxCodeId` have no explicit index definitions in configurations. EF Core does not auto-create FK indexes. File: `AE.Market.Infrastructure/Persistence/Configurations/Catalog/ProductConfiguration.cs`
- **Missing `AttributeGroup` configuration** -- `Category.AttributeGroups` is mapped but there is no `AttributeGroupConfiguration.cs` in the Configurations/Catalog folder. The `CategoryConfiguration.cs` does not configure `HasMany(x => x.AttributeGroups)`. File: glob search confirms no `AttributeGroupConfiguration.cs`
- **`AppDbContext` does not expose `DbSet<AttributeGroup>`** -- If AttributeGroups are to be queried directly, they need a DbSet. File: `AE.Market.Infrastructure/Persistence/AppDbContext.cs`
- **`ChangeParent` on `Category` doesn't validate that ParentId references an existing Category** -- the domain just checks it's not self/descendant. Validation of existence is pushed to the application handler but currently no handler validates it.
- **Outbox interval is 100 seconds** (not 5s as BACKEND_PLAN.md documents). This is a known gap documented in AGENTS.md. File: `AE.Market.Infrastructure/DependencyInjection.cs:87`

---

## ISSUES

| Severity | Issue | File |
|----------|-------|------|
| **Critical** | No Catalog migration; `EnsureDeleted/EnsureCreated` is dev-only, not production-safe | `Program.cs:64-73`, `Migrations/` |
| **Critical** | `ShippingDimensions` owned entity configured but not defined in `Product` domain entity | `ProductConfiguration.cs:38-43`, `Product.cs` |
| **High** | No soft-delete query filter on any Catalog entity | All configuration files |
| **High** | Zero integration tests for Catalog operations | `tests/AE.Market.Integration.Tests/` |
| **High** | Empty CI/CD workflow directory | `.github/workflows/` |
| **Medium** | `Category.ChangeParent()` throws `DomainException` instead of Result pattern | `Category.cs:112-113` |
| **Medium** | Event handlers perform unnecessary DB queries for cache invalidation | `ProductDeletedEventHandler.cs`, `ProductUpdatedEventHandler.cs` |
| **Medium** | Inconsistent domain event naming (`Event` vs `DomainEvent` suffix) | `CategoryCreatedEvent.cs` |
| **Medium** | Missing `AttributeGroupConfiguration.cs` and `DbSet<AttributeGroup>` | Configurations/Catalog/, AppDbContext.cs |
| **Low** | Missing FK indexes on Product navigation properties | `ProductConfiguration.cs` |
| **Low** | Outbox polling interval (100s) differs from documented spec (5s) | `DependencyInjection.cs:87` |
| **Low** | `Category.ChangeParent()` doesn't validate existence of parent ID | `Category.cs`, no handler validation |

---

## Recommendations (Priority-Ordered)

### 1. [CRITICAL] Add EF Core migration for Catalog schema

Stop relying on `EnsureDeleted/EnsureCreated()`. Generate a proper migration that creates the `catalog` schema with all 12+ tables. This is the single biggest blocker to production deployment.

```bash
dotnet ef migrations add AddCatalogSchema --project AE.Market.Infrastructure --startup-project AE.Market.API
```

Then update `Program.cs` to use `MigrateAsync()` instead of `EnsureDeleted/EnsureCreated()`.

### 2. [CRITICAL] Fix ShippingDimensions configuration mismatch

Either add the `ShippingDimensions` owned entity to `Product.cs` or remove the `OwnsOne` configuration from `ProductConfiguration.cs`. As-is, this will crash on any attempt to query Products.

### 3. [HIGH] Add global soft-delete query filter

In `AppDbContext.OnModelCreating()`, apply a convention-based query filter for all entities inheriting `BaseEntity`:

```csharp
modelBuilder.Model.GetEntityTypes()
    .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType))
    .ToList()
    .ForEach(e => modelBuilder.Entity(e.ClrType)
        .HasQueryFilter(nameof(BaseEntity.IsDeleted), false));
```

### 4. [HIGH] Add Catalog integration tests

Create `CatalogIntegrationTests.cs` following the `AuthIntegrationTests.cs` pattern. At minimum test: create category, create brand, create product with variant, query by slug, delete product. The `IntegrationTestWebAppFactory` and Testcontainers infrastructure is already in place.

### 5. [MEDIUM] Add Missing EF configurations and DbSets

Create `AttributeGroupConfiguration.cs` and add `DbSet<AttributeGroup>` to `AppDbContext`. Add explicit indexes for `Product.BrandId`, `Product.CategoryId`, and `Product.TaxCodeId` in `ProductConfiguration.cs`.

### 6. [MEDIUM] Refactor Category.ChangeParent() and event naming

- Change `Category.ChangeParent()` to return a `Result` instead of throwing `DomainException`.
- Rename `CategoryCreatedEvent` to `CategoryCreatedDomainEvent` to pass the architecture test.

### 7. [MEDIUM] Optimize cache invalidation in event handlers

Include `Slug`, `CategoryId`, `BrandId` in the domain event payload so event handlers don't need to re-fetch the entity just for cache key construction. For example, `ProductDeletedDomainEvent` should carry the product's slug and category ID.

---

## Production Readiness Score

# **6.5 / 10**

### Breakdown
| Category | Score | Rationale |
|----------|-------|-----------|
| **Domain Model** | 9/10 | Rich, well-encapsulated, proper value objects, 53 events. Deduction for ChangeParent() throwing. |
| **Application Layer** | 8/10 | Full CQRS, validators, specs, cache keys, event handlers. Deduction for inefficient cache invalidation queries. |
| **API Layer** | 9/10 | All controllers exist, proper auth, REST conventions. Minor: no pagination on list endpoints. |
| **Infrastructure** | 5/10 | Configurations exist but missing soft-delete filters, missing AttributeGroup config, ShippingDimensions mismatch, no production-safe migration. |
| **Testing** | 5/10 | Domain tests are good; architecture tests are good; integration tests cover **Auth only** (zero Catalog). |
| **Production Readiness** | 3/10 | No CI, no production migration for Catalog, EF configuration bug will crash at runtime, no deploy pipeline. |
| **Documentation** | 7/10 | AGENTS.md, BACKEND_PLAN.md, and 15 review docs in `docs/catalog-review/` are thorough. |

### Summary
The Catalog aggregate is **architecturally sound and feature-complete from a code perspective**, but it is **not production-ready** due to the `ShippingDimensions` configuration bug that would crash the application, the lack of a proper migration strategy, and zero integration test coverage. With focused effort on the top 5 recommendations, this could reach an **8.5/10** within a sprint.
