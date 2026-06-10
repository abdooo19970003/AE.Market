# AeMarket Backend — .NET / C# Implementation Plan

> Production-ready e-commerce backend · Modular monolith · Clean Architecture · CQRS · DDD
> Timeline: **12 sprints (~24 weeks)** · Each sprint ships a **runnable feature** with tests
> Stack: ASP.NET Core 10, EF Core 10, PostgreSQL 17, Elasticsearch 8, Redis, Docker

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Technology Stack](#technology-stack)
3. [Project Structure](#project-structure)
4. [Key Decisions](#key-decisions)
5. [Domain Event & Outbox Pattern](#domain-event--outbox-pattern)
6. [Sprint Plan](#sprint-plan)
7. [Database Schema Map](#database-schema-map)
8. [Cross-Domain Contracts](#cross-domain-contracts)
9. [API Design](#api-design)
10. [Non-Functional Requirements](#non-functional-requirements)

---

## Architecture Overview

```
AeMarket.sln
├── src/
│   ├── AeMarket.Domain/              # Entities, VOs, enums, events — organized by aggregate folder
│   ├── AeMarket.Application/         # Commands, Queries, DTOs, Validation — organized by feature folder
│   ├── AeMarket.Infrastructure/      # EF Core, ES, Redis, ImageKit, JWT — organized by concern
│   └── AeMarket.Api/                 # Controllers, Middleware, Program.cs
├── tests/
│   ├── AeMarket.Domain.Tests/
│   ├── AeMarket.Application.Tests/
│   └── AeMarket.Api.Tests/           # Integration tests with Testcontainers
├── docker-compose.yml
├── Dockerfile
└── .github/workflows/ci.yml
```

**5 projects, 1 solution.** No service boundaries between projects — boundaries are in **folders and namespace conventions**. Each sprint adds to every layer: Domain → Application → Infrastructure → Api → Tests.

### Folder Organization by Domain

```
AeMarket.Domain/
├── Common/                           # Base entity, value object, domain event interfaces
├── Aggregates/
│   ├── Auth/                         # User, RefreshToken, Email, PasswordHash
│   ├── Catalog/                      # Category, Product, ProductVariant, Images
│   ├── Catalog/Attributes/           # CategoryAttribute, AttributeOption, ProductAttributeValue
│   ├── Pricing/                      # VariantPrice, PriceHistory, Money
│   ├── Inventory/                    # StockEntry
│   ├── Cart/                         # Cart, CartItem
│   └── Orders/                       # Order, OrderItem, OrderStatus

AeMarket.Application/
├── Common/                           # Behaviors, interfaces, BaseHandler
├── Features/
│   ├── Auth/                         # Commands/, Queries/, DTOs/, Validators/
│   ├── Categories/
│   ├── Products/
│   ├── Variants/
│   ├── Attributes/
│   ├── Pricing/
│   ├── Inventory/
│   ├── Cart/
│   ├── Orders/
│   ├── Search/
│   ├── SEO/
│   └── Admin/
```

---

## Technology Stack

| Category | Choice | Rationale |
|---|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 | Latest, compiled queries, minimal API, AOT ready |
| ORM | EF Core 10 | `AsNoTracking` + `SplitQuery` + compiled queries = Dapper-level read perf |
| CQRS | MediatR + FluentValidation | Industry standard pipeline |
| Mapping | Mapster | Faster than AutoMapper, less ceremony |
| Database | PostgreSQL 17 | Recursive CTEs, JSONB, full-text search |
| Schemas | `auth`, `catalog`, `pricing`, `inventory`, `cart`, `orders` | Logical separation in one DB |
| Search | Elasticsearch 8 | Faceted search, autocomplete, synonyms |
| Cache | FusionCache + Redis | Stampede protection, failback, mem + distributed |
| Logging | Serilog → Console + ES sink | Structured, searchable |
| Tracing | OpenTelemetry | Vendor-neutral, Prometheus / OTLP |
| Auth | Custom JWT + Permission-based (not ASP.NET Identity) | Fine-grained `Permission` enum, no rigid roles |
| Outbox | EF interceptor + Quartz.NET | Reliable event delivery with retry + dead-letter |
| Jobs | Quartz.NET | Nightly ES re-index, outbox processor |
| Container | Docker Compose | Dev/prod parity |
| CI/CD | GitHub Actions | Build → Test → Publish → Deploy |

---

## Project Structure (Full)

```
AeMarket.sln
│
├── src/
│   ├── AeMarket.Domain/
│   │   ├── AeMarket.Domain.csproj
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs              # Id, DomainEvents list, concurrency token
│   │   │   ├── ValueObject.cs             # Base class for VOs
│   │   │   ├── IDomainEvent.cs            # Marker interface
│   │   │   ├── IAggregateRoot.cs          # Marker interface
│   │   │   └── Guard.cs                   # Parameter validation helpers
│   │   ├── Aggregates/
│   │   │   ├── Auth/
│   │   │   │   ├── User.cs
│   │   │   │   ├── RefreshToken.cs
│   │   │   │   ├── UserProfile.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── UserRegisteredEvent.cs
│   │   │   │   │   └── UserLoggedInEvent.cs
│   │   │   │   └── ValueObjects/
│   │   │   │       ├── Email.cs
│   │   │   │       └── PasswordHash.cs
│   │   │   ├── Catalog/
│   │   │   │   ├── Category.cs
│   │   │   │   ├── Product.cs
│   │   │   │   ├── ProductVariant.cs
│   │   │   │   ├── ProductImage.cs
│   │   │   │   ├── VariantImage.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── ProductCreatedEvent.cs
│   │   │   │   │   ├── ProductUpdatedEvent.cs
│   │   │   │   │   └── VariantAddedEvent.cs
│   │   │   │   └── ValueObjects/
│   │   │   │       ├── Slug.cs
│   │   │   │       └── Sku.cs
│   │   │   ├── Catalog/Attributes/
│   │   │   │   ├── CategoryAttribute.cs
│   │   │   │   ├── AttributeOption.cs
│   │   │   │   ├── ProductAttributeValue.cs
│   │   │   │   ├── VariantAttributeValue.cs
│   │   │   │   └── AttributeInputType.cs
│   │   │   ├── Pricing/
│   │   │   │   ├── VariantPrice.cs
│   │   │   │   ├── PriceHistory.cs
│   │   │   │   ├── Money.cs
│   │   │   │   ├── PriceChangeReason.cs
│   │   │   │   └── Events/
│   │   │   │       └── ProductPriceChangedEvent.cs
│   │   │   ├── Inventory/
│   │   │   │   ├── StockEntry.cs
│   │   │   │   └── Events/
│   │   │   │       └── StockAdjustedEvent.cs
│   │   │   ├── Cart/
│   │   │   │   ├── Cart.cs
│   │   │   │   ├── CartItem.cs
│   │   │   │   └── Events/
│   │   │   │       └── ItemAddedToCartEvent.cs
│   │   │   └── Orders/
│   │   │       ├── Order.cs
│   │   │       ├── OrderItem.cs
│   │   │       ├── OrderStatus.cs
│   │   │       └── Events/
│   │   │           └── OrderPlacedEvent.cs
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       └── NotFoundException.cs
│   │
│   ├── AeMarket.Application/
│   │   ├── AeMarket.Application.csproj
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── TransactionBehavior.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IRepository.cs
│   │   │   │   ├── IReadRepository.cs
│   │   │   │   ├── IJwtService.cs
│   │   │   │   ├── ICacheService.cs
│   │   │   │   ├── IElasticsearchService.cs
│   │   │   │   └── IImageService.cs
│   │   │   ├── DTOs/
│   │   │   │   ├── PagedResult.cs
│   │   │   │   └── ErrorResponse.cs
│   │   │   ├── Mappings/
│   │   │   │   └── MappingProfile.cs
│   │   │   └── Models/
│   │   │       └── PaginationParams.cs
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   │   ├── CommandsregisterregisterCommand.cs + Handler + Validator
│   │   │   │   ├── QueriesloginloginQuery.cs + Handler + Validator
│   │   │   │   └── DTOs/TokenResponse.cs, UserDto.cs
│   │   │   ├── Categories/
│   │   │   │   ├── Commands/CreateCategory/
│   │   │   │   ├── Commands/UpdateCategory/
│   │   │   │   ├── Commands/DeleteCategory/
│   │   │   │   ├── Queries/GetCategoryTree/
│   │   │   │   └── Queries/GetCategoryBySlug/
│   │   │   ├── Products/
│   │   │   │   ├── Commands/CreateProduct/
│   │   │   │   ├── Commands/UpdateProduct/
│   │   │   │   ├── Commands/DeleteProduct/
│   │   │   │   ├── Queries/GetProduct/
│   │   │   │   ├── Queries/GetProductDetail/
│   │   │   │   └── Queries/ListProducts/
│   │   │   ├── Variants/
│   │   │   ├── Attributes/
│   │   │   ├── Pricing/
│   │   │   ├── Inventory/
│   │   │   ├── Cart/
│   │   │   ├── Orders/
│   │   │   ├── Search/
│   │   │   ├── SEO/
│   │   │   └── Admin/
│   │   └── DependencyInjection.cs
│   │
│   ├── AeMarket.Infrastructure/
│   │   ├── AeMarket.Infrastructure.csproj
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Repository/
│   │   │   │   ├── Repository.cs
│   │   │   │   └── SpecificationEvaluator.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── Auth/
│   │   │   │   ├── Catalog/
│   │   │   │   ├── Pricing/
│   │   │   │   ├── Inventory/
│   │   │   │   ├── Cart/
│   │   │   │   └── Orders/
│   │   │   ├── Interceptors/
│   │   │   │   ├── DomainEventDispatcher.cs       # Collects + stores in outbox
│   │   │   │   └── AuditInterceptor.cs            # CreatedAt/UpdatedAt auto-set
│   │   │   ├── Outbox/
│   │   │   │   ├── OutboxMessage.cs
│   │   │   │   ├── OutboxConfiguration.cs
│   │   │   │   └── OutboxProcessorJob.cs          # Quartz.NET: polls + publishes
│   │   │   ├── CompiledQueries.cs
│   │   │   └── Migrations/
│   │   ├── Auth/
│   │   │   ├── JwtService.cs
│   │   │   └── PasswordService.cs
│   │   ├── Search/
│   │   │   ├── ElasticsearchService.cs
│   │   │   ├── ProductIndexDefinition.cs
│   │   │   ├── IndexProductHandler.cs             # MediatR notification handler
│   │   │   └── SyncProductsJob.cs                 # Quartz.NET nightly
│   │   ├── Caching/
│   │   │   └── CacheService.cs
│   │   ├── Images/
│   │   │   └── ImageKitService.cs
│   │   └── DependencyInjection.cs
│   │
│   └── AeMarket.Api/
│       ├── AeMarket.Api.csproj
│       ├── Program.cs
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── CategoriesController.cs
│       │   ├── ProductsController.cs
│       │   ├── CartController.cs
│       │   ├── OrdersController.cs
│       │   ├── SearchController.cs
│       │   ├── AdminController.cs
│       │   └── PublicController.cs                 # Sitemap, health
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs      # Result → HTTP status mapping
│       │   ├── RequestLoggingMiddleware.cs
│       │   ├── SecurityHeadersMiddleware.cs
│       │   └── CorrelationIdMiddleware.cs
│       └── appsettings.json
│
├── tests/
│   ├── AeMarket.Domain.Tests/
│   ├── AeMarket.Application.Tests/
│   └── AeMarket.Api.Tests/                        # Testcontainers for integration
│
├── docs/
│   ├── decisions.md                                # Architecture Decision Records
│   ├── runbook.md
│   └── benchmarks.md
│
├── docker-compose.yml
├── docker-compose.prod.yml
├── Dockerfile
└── .github/workflows/ci.yml
```

---

## Key Decisions

### Result Pattern — FluentResults

**All handlers return `Result<T>` or `Result`.** No throwing exceptions for control flow.

```csharp
public async Task<Result<ProductDto>> Handle(CreateProductCommand cmd, CancellationToken ct)
{
    var slug = Slug.From(cmd.Name);
    if (await _db.Products.AnyAsync(p => p.Slug == slug, ct))
        return Result.Fail(new SlugConflictError(slug));

    var product = new Product(cmd.Name, slug, cmd.CategoryId);
    _db.Products.Add(product);
    await _db.SaveChangesAsync(ct);
    return product.Adapt<ProductDto>().ToResult();
}
```

**Controller maps Result → HTTP:**
- `Result.Ok(value)` → `200 OK`
- `Result.Fail(NotFoundError)` → `404 Not Found`
- `Result.Fail(ForbiddenError)` → `403 Forbidden`
- `Result.Fail(ValidationError)` → `400 Bad Request / 422 Unprocessable`
- `Result.Fail(ConflictError)` → `409 Conflict`

### Result of Exceptions

Exceptions are only thrown for **programming errors** (null arguments where they shouldn't be, impossible states). Domain logic errors, validation failures, and "not found" are all `Result.Fail`.

### Two-Repository Pattern

| Repository | Tracking | Used For |
|---|---|---|
| `IRepository<T>` | Default (tracked) | Commands, writes, outbox |
| `IReadRepository<T>` | `AsNoTracking()` | All read queries |

Both implemented by the same `Repository<T>` class behind the scenes—read methods always apply `AsNoTracking()`. Shared `IEntityTypeConfiguration<T>` classes. Same `AppDbContext`, same connection string. Queries go through `SpecificationEvaluator<T>` (criteria, includes, ordering, pagination).

### Cart Persistence — PostgreSQL Only

- Guest carts: `SessionId` GUID stored in cookie, `Cart.UserId` is null
- Authenticated carts: `Cart.UserId` links to `auth.users`
- Both live in the same `cart.carts` + `cart.cart_items` tables

### API Versioning

- v1 assumed by default, no prefix
- When breaking changes are needed: `Accept: application/vnd.aemarket.v2+json` header
- Decision documented in ADR, not implemented until necessary

### Migration Strategy

| Environment | How |
|---|---|
| **Development** | `app.EnsureMigrated()` on startup (convenience, acceptable risk) |
| **CI/CD** | `dotnet ef database update` runs as **separate init container** in Docker Compose |
| **Production** | Init container only — app never runs migrations |

---

## Domain Event & Outbox Pattern

This is the **backbone of cross-domain communication** without coupling.

### Lifecycle of a Domain Event

```
1. Entity.AddDomainEvent(new ProductCreatedEvent(product.Id))
       ↓
2. SaveChangesAsync()
       ↓
3. SaveChangesInterceptor (DomainEventDispatcher)
       ├── BeforeSave: collect events from tracked entities
       └── AfterSave: write events to outbox_messages table
            └── entity.ClearDomainEvents()
       ↓
4. OutboxProcessorJob (Quartz.NET, polls every 5s)
       ├── SELECT FROM outbox_messages WHERE processed_on IS NULL
       ├── ORDER BY occurred_on
       ├── FOR UPDATE SKIP LOCKED                         ← prevents double-processing
       ├── For each event:
       │     try:
       │       Deserialize event
       │       _publisher.Publish(event)                   ← MediatR in-process
       │       SET processed_on = now()
       │     catch:
       │       retry_count++, backoff = retry^2 * 100ms
       │       IF retry_count > 10 → dead_letter
       └── COMMIT
```

### Event Catalog

| Event | Publisher | Subscribers | Sprint |
|---|---|---|---|
| `UserRegisteredEvent` | Auth | — (future: email service) | 1 |
| `ProductCreatedEvent` | Catalog | Search (index), Cache (warm) | 3, 9, 10 |
| `ProductUpdatedEvent` | Catalog | Search (re-index), Cache (invalidate) | 3, 9, 10 |
| `ProductPriceChangedEvent` | Pricing | Search (re-index), Cache (invalidate) | 5, 9, 10 |
| `StockAdjustedEvent` | Inventory | — (future: order fulfillment) | 6 |
| `ItemAddedToCartEvent` | Cart | Analytics (track) | 7, 11 |
| `OrderPlacedEvent` | Orders | Inventory (reserve), Analytics | 8, 11 |

### Outbox Schema

```sql
CREATE TABLE outbox.outbox_messages (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_id    VARCHAR(255) NOT NULL,
    event_type      VARCHAR(500) NOT NULL,       -- Full type name, e.g. "AeMarket.Domain.Aggregates.Catalog.Events.ProductCreatedEvent"
    payload         JSONB NOT NULL,              -- Serialized event
    occurred_on     TIMESTAMPTZ NOT NULL,
    processed_on    TIMESTAMPTZ,                 -- NULL = pending
    retry_count     INT NOT NULL DEFAULT 0,
    error           TEXT,                        -- Last error message, for debugging
    dead_letter     BOOLEAN NOT NULL DEFAULT false
);

CREATE INDEX idx_outbox_unprocessed ON outbox.outbox_messages (occurned_on)
    WHERE processed_on IS NULL AND dead_letter = false;
```

---

## Sprint Plan — 12 Sprints

Each sprint builds on the previous. Every sprint ships:
- **Runnable API endpoints** that can be called
- **Tests** (unit + integration)
- **Migrations** for new tables
- **Outbox wiring** if new domain events are introduced

---

### Sprint 1 — Auth & Identity

**Runnable:** User registration, login, JWT refresh, current user.

| Area | Deliverables |
|---|---|
| **Domain** | `User`, `RefreshToken`, `UserProfile`, `UserPermission` entities; `Permission` enum (`AccessUsers`, `MutateUsers`); `Email`, `PasswordHash` VOs; `UserRegisteredEvent` |
| **Application** | `RegisterCommand` (validate email uniqueness, password policy), `LoginQuery`, `RefreshTokenCommand`, `LogoutCommand`; FluentValidation for all |
| **Infrastructure** | `AppDbContext` + `Repository<T>` (implements both `IRepository<T>` and `IReadRepository<T>`); `auth` schema; `UserConfiguration`, `RefreshTokenConfiguration`; `JwtService` (access + refresh tokens), `PasswordService` (bcrypt) |
| **Api** | `AuthController`: `POST /api/authregister`, `POST /api/authlogin`, `POST /api/authrefresh`, `POST /api/auth/logout`, `GET /api/authme`; `ExceptionHandlingMiddleware` with Result → HTTP mapping |
| **Tests** | Unit: password hashing, token generation/validation. Integration: register → login → verify token → refresh → me |

**Outbox:** Not yet — Auth has no cross-context subscribers in sprint 1. The interceptor is wired but idle.

**Result pattern:** All handlers return `Result<T>` or `Result`. Controllers use helper to map.

---

### Sprint 2 — Categories

**Runnable:** Category CRUD + tree with permission guard.

| Area | Deliverables |
|---|---|
| **Domain** | `Category` entity (self-referencing parentId); `CategoryCreatedEvent` |
| **Application** | `CreateCategoryCommand` (slug generation, parent validation), `UpdateCategoryCommand`, `DeleteCategoryCommand` (prevent if products exist); `GetCategoryTreeQuery` (recursive CTE via raw SQL or `FromSql`), `GetCategoryBySlugQuery` |
| **Infrastructure** | `CategoryConfiguration`; `catalog` schema; compiled query for root categories; `catalog` schema added to `AppDbContext` |
| **Api** | `CategoriesController`: `GET /api/categories` (tree), `GET /api/categories/{slug}`, `POST /api/categories`, `PATCH /api/categories/{id}`, `DELETE /api/categories/{id}`; permission check on write endpoints |
| **Tests** | Integration: create root + sub-categories → query tree → verify nesting, prevent delete with products |

**Cross-context:** `RequirePermission(MutateUsers)` policy consumed from Application/Common.

---

### Sprint 3 — Products & Variants

**Runnable:** Product + variant CRUD, image upload, paginated product list.

| Area | Deliverables |
|---|---|
| **Domain** | `Product`, `ProductVariant`, `ProductImage`, `VariantImage` entities; `Slug`, `Sku` VOs; `ProductCreatedEvent`, `ProductUpdatedEvent`, `VariantAddedEvent` |
| **Application** | Full CRUD commands + queries for products and variants; `ListProductsQuery` with pagination, sort, filter by category/text; image upload command |
| **Infrastructure** | Product/variant/image configs (indexed FKs, split query config); ImageKit service for upload; `catalog` schema extended |
| **Api** | `ProductsController`: `GET /api/products` (paginated), `GET /api/products/{id}`, `GET /api/products/{slug}`, `POST /api/admin/products`, `PATCH /api/admin/products/{id}`, `DELETE /api/admin/products/{id}`; variant sub-routes; image upload |
| **Tests** | Unit: slug generation, SKU format validation. Integration: create product with 3 variants → paginate list → filter by category |

**Outbox:** `ProductCreatedEvent` published. No subscriber yet — comes in sprint 9 (Search). Outbox table and processor job are wired.

---

### Sprint 4 — Attributes (EAV)

**Runnable:** Dynamic attribute definitions per category, attribute values on products and variants.

| Area | Deliverables |
|---|---|
| **Domain** | `CategoryAttribute`, `AttributeOption`, `ProductAttributeValue`, `VariantAttributeValue` entities; `AttributeInputType` enum (text, number, select, multi_select, boolean) |
| **Application** | Attribute definition CRUD commands + queries; `SetProductAttributeValueCommand`, `SetVariantAttributeValueCommand` (upsert); full `GetProductDetailQuery` (assembles: product + category breadcrumbs + attributes + variants + prices + images) |
| **Infrastructure** | Attribute configs in `catalog` schema; validation interceptor for required attributes; split query for detail endpoint |
| **Api** | `AttributesController` (admin); updated `GET /api/products/{id}/detail` returns full nested response |
| **Tests** | Integration: define attributes → add options → assign to product → verify detail response. Negative: missing required attribute rejected, wrong attribute for category rejected |

---

### Sprint 5 — Pricing

**Runnable:** Set/update variant prices, audit trail, margin queries.

| Area | Deliverables |
|---|---|
| **Domain** | `VariantPrice`, `PriceHistory` entities; `Money` VO (amount + currency, comparison operators); `PriceChangeReason` enum; `ProductPriceChangedEvent` |
| **Application** | `SetInitialPriceCommand`, `UpdatePriceCommand` (deactivate old → insert new → log history), `GetActivePriceQuery`, `GetPriceHistoryQuery` (date range, paginated), `GetMarginQuery` (cost vs sell, percentage) |
| **Infrastructure** | `pricing` schema; `VariantPriceConfiguration` with **partial unique index** `WHERE is_active = true` — enforces one active price per variant at DB level; `PriceHistory.ChangedBy` made **nullable** (carries forward the fix from the current schema's known bug) |
| **Api** | `PricingController` (admin sub-routes under `products/{id}/variants/{vid}/price`) |
| **Tests** | Integration: set initial price → update price → verify history has 2 entries → try to activate second price → DB constraint rejects |

**Outbox:** `ProductPriceChangedEvent` published → no subscriber yet (sprint 9).

---

### Sprint 6 — Inventory

**Runnable:** Stock tracking, low-stock alerts, stock adjustment.

| Area | Deliverables |
|---|---|
| **Domain** | `StockEntry` entity (variantId, quantity, reservedQuantity, lowStockThreshold); `StockAdjustedEvent` |
| **Application** | `AdjustStockCommand` (add/reduce/set), `ReserveStockCommand`, `ReleaseStockCommand`, `GetStockQuery`, `GetLowStockReportQuery` |
| **Infrastructure** | `inventory` schema; `StockEntryConfiguration`; optimistic concurrency for stock updates (row version) |
| **Api** | `InventoryController` (admin): `GET /api/admin/inventory`, `PATCH /api/admin/inventory/{variantId}/adjust`, `GET /api/admin/inventory/low-stock` |
| **Tests** | Integration: adjust stock → verify quantity → reserve → verify reserved count → release |

---

### Sprint 7 — Cart

**Runnable:** Guest + authenticated shopping cart, merge on login.

| Area | Deliverables |
|---|---|
| **Domain** | `Cart` (userId nullable, sessionId, status), `CartItem` (variantId, quantity, addedAt); `ItemAddedToCartEvent`, `CartMergedEvent` |
| **Application** | `AddToCartCommand` (upsert: increment if exists, insert if new), `RemoveFromCartCommand`, `UpdateCartItemQuantityCommand`, `GetCartQuery`, `MergeGuestCartCommand` (transfers items to user on login) |
| **Infrastructure** | `cart` schema; cart configs; guest session by `SessionId` cookie (set by middleware) |
| **Api** | `CartController`: `GET /api/cart`, `POST /api/cart/items`, `PATCH /api/cart/items/{variantId}`, `DELETE /api/cart/items/{variantId}`, `POST /api/cartmerge` |
| **Tests** | Integration: guest add items → register → merge cart → verify all items transferred; duplicate add increments quantity |

---

### Sprint 8 — Orders (with Idempotency)

**Runnable:** Place order with idempotency, order history, status tracking.

| Area | Deliverables |
|---|---|
| **Domain** | `Order` (orderNumber, userId, status, total, shippingAddress), `OrderItem` (variantId, productSnapshot, priceSnapshot, quantity), `OrderStatus` enum (pending, confirmed, shipped, delivered, cancelled); `OrderPlacedEvent` |
| **Application** | `PlaceOrderCommand` (with **idempotency key**), `CancelOrderCommand`, `GetOrderQuery`, `GetOrderHistoryQuery` |
| **Infrastructure** | `orders` schema; `OrderConfiguration`, `OrderItemConfiguration` (price snapshot as Money); `IdempotencyRequest` table (key, response, created) — duplicate key returns cached response |
| **Api** | `OrdersController`: `POST /api/orders` (requires `Idempotency-Key` header), `GET /api/orders`, `GET /api/orders/{id}`, `POST /api/orders/{id}/cancel`; admin: `GET /api/admin/orders` |
| **Tests** | Integration: place order → verify order snapshot has correct prices → submit same idempotency key → only one order created → verify stock reduced |

**Idempotency flow:**
```
Request with Idempotency-Key
  → Check idempotency_requests
    → Found: return cached 201 response (idempotent replay)
    → Not found: process order → store key + response in idempotency table → return 201
```

---

### Sprint 9 — Search (Elasticsearch)

**Runnable:** Full-text product search with faceted filters, autocomplete suggestions.

| Area | Deliverables |
|---|---|
| **Application** | `SearchProductsQuery` (q, filters, facets, sort, page), `SearchSuggestQuery` (completion suggester), `IndexProductHandler` (MediatR `INotificationHandler` for `ProductCreatedEvent`, `ProductUpdatedEvent`, `ProductPriceChangedEvent`) |
| **Infrastructure** | `ElasticsearchService` (official v8 SDK); `ProductIndexDefinition` (analyzer, field mappings, nested attributes); `SyncProductsJob` (Quartz.NET, nightly full re-index) |
| **Api** | `SearchController`: `GET /api/search?q=iphone&categoryId=1&minPrice=500&maxPrice=1500&sort=price_asc&page=1&size=20`, `GET /api/search/suggest?q=ipho` |
| **Tests** | Integration with ES Testcontainer: index product → search by name → facet by category → autocomplete partial input |

**Payoff:** The outbox has been accumulating events since sprint 3. Now Search subscribes to them. No code changes in Catalog or Pricing needed.

---

### Sprint 10 — SEO & Caching

**Runnable:** Sitemap, FusionCache on product detail + category tree, cache invalidation via events.

| Area | Deliverables |
|---|---|
| **Domain** | SEO fields added to Product: `MetaTitle`, `MetaDescription`, `MetaKeywords`, `OgImage`; `ProductViewedEvent` (for analytics in sprint 11) |
| **Application** | `GetSitemapQuery` (generates `sitemap.xml` with products + categories), cache invalidation handlers for `ProductUpdatedEvent`, `ProductPriceChangedEvent`, `CategoryChangedEvent` |
| **Infrastructure** | `FusionCacheService` with Redis + memory layer; `CacheConfiguration` with default entry options (10 min, stampede protection); cache-aside on product detail, category tree, product list (by filter hash) |
| **Api** | `GET /sitemap.xml` (XML content type, response cached for 1 hour) |
| **Tests** | Unit: cache hit/miss behavior. Integration: update product → verify cache invalidated → fresh data returned |

---

### Sprint 11 — Analytics & Observability

**Runnable:** Admin dashboard stats, structured logging, distributed tracing, health checks.

| Area | Deliverables |
|---|---|
| **Application** | `GetAdminStatsQuery` (total products, active stock count, average sell price, categories, products by category, top 10 viewed products, top searches) |
| **Infrastructure** | Serilog (Console + ES sink), OpenTelemetry (`AspNetCore` + `EntityFrameworkCore` instrumentation), `ProductViewEventHandler` (increments view counter in DB), `SearchLoggedEventHandler` (logs search queries for analytics) |
| **Api** | `AdminController`: `GET /api/admin/stats`, `GET /api/admin/stats/top-products?days=30`, `GET /api/admin/stats/top-searches?days=30`; `GET /health` (liveness), `GET /health/ready` (PG + Redis + ES checks) |
| **Tests** | Integration: view product → verify analytics event recorded → stats endpoint reflects it |

---

### Sprint 12 — Production Hardening

**Runnable:** CI/CD pipeline, Docker optimization, security audit, documentation.

| Area | Deliverables |
|---|---|
| **Docker** | Multi-stage Dockerfile (chiseled `aspnet:10.0`, `USER $APP_UID`, non-root); `docker-compose.prod.yml` with init container for migrations, reverse proxy, resource limits, secrets via env file |
| **CI/CD** | `.github/workflows/ci.yml`: trigger on push/PR to `main`; steps: restore → build → unit tests → integration tests (Testcontainers) → Docker build → push to GHCR |
| **Security** | `SecurityHeadersMiddleware` (CSP, HSTS, X-Frame-Options, X-Content-Type-Options); CORS policy for admin domain; rate limiting review; JWT rotation enforcement |
| **Docs** | Swagger + Scalar UI; `docs/decisions.md` (ADRs for 10 key decisions: outbox, result pattern, modular monolith, no Dapper, FusionCache, etc.); `docs/runbook.md` (startup, health checks, failure modes, rollback) |
| **Load testing** | `k6` script for: product list (200 concurrent), product detail (200), search (100), order placement (50); target p95 < 500ms; bottleneck report |

---

## Bonus Sprints

| Sprint | Feature | Key Entities | Depends On |
|---|---|---|---|
| 13 | Promotions & Coupons | `Promotion`, `Coupon`, `PromotionProduct` | Catalog, Pricing |
| 14 | Reviews & Ratings | `Review`, `AggregatedRating` | Catalog, Auth |
| 15 | Wishlist & Compare | `WishlistItem`, `CompareSession` | Catalog, Auth |
| 16 | B2B Features | `CustomerGroup`, `PriceList`, `ApprovalRule` | Catalog, Pricing, Orders |

---

## Database Schema Map

### Schema: `auth`

| Table | Key Columns | Sprint |
|---|---|---|
| `users` | `id`, `email` (unique), `password_hash`, `is_active`, `created_at`, `updated_at` | 1 |
| `user_permissions` | `user_id` FK, `permission` (int), `created_at` | 1 |
| `permissions` | `id`, `name` (unique), `description` | 1 |
| `refresh_tokens` | `id`, `user_id` FK, `token_hash`, `expires_at`, `revoked_at` | 1 |
| `user_profiles` | `id`, `user_id` FK (unique), `first_name`, `last_name`, `phone`, `address`, `city`, `country`, `image_url` | 1 |

### Schema: `catalog`

| Table | Key Columns | Sprint |
|---|---|---|
| `categories` | `id`, `name`, `slug` (unique), `parent_id` FK (self), `description`, `image_url`, `is_active`, `sort_order` | 2 |
| `category_attributes` | `id`, `category_id` FK, `name`, `slug`, `input_type`, `unit`, `is_required`, `is_filterable`; unique `(category_id, slug)` | 4 |
| `attribute_options` | `id`, `attribute_id` FK, `label`, `value`, `sort_order`; unique `(attribute_id, value)` | 4 |
| `products` | `id`, `category_id` FK (restrict), `name`, `slug` (unique), `sku` (unique), `description`, `is_active`, `meta_title`, `meta_description`, `meta_keywords` | 3 |
| `product_variants` | `id`, `product_id` FK (cascade), `name`, `sku` (unique), `stock_quantity`, `is_active` | 3 |
| `product_images` | `id`, `product_id` FK (cascade), `url`, `alt_text`, `is_primary`, `sort_order` | 3 |
| `variant_images` | `id`, `variant_id` FK (cascade), `url`, `alt_text`, `is_primary`, `sort_order` | 3 |
| `product_attribute_values` | `id`, `product_id` FK, `attribute_id` FK, `value_text`, `value_option_id` FK; unique `(product_id, attribute_id)` | 4 |
| `variant_attribute_values` | `id`, `variant_id` FK, `attribute_id` FK, `value_text`, `value_option_id` FK; unique `(variant_id, attribute_id)` | 4 |

### Schema: `pricing`

| Table | Key Columns | Sprint |
|---|---|---|
| `variant_prices` | `id`, `variant_id` FK (cascade), `cost_price` (numeric 12,2), `sell_price` (numeric 12,2), `is_active`, `effective_at`; **partial unique index WHERE `is_active = true`**; check constraints `>= 0` | 5 |
| `price_history` | `id`, `variant_id` FK, `old_cost_price`, `new_cost_price`, `old_sell_price`, `new_sell_price`, `reason`, `note`, `changed_by` (nullable) FK users, `changed_at` | 5 |

### Schema: `inventory`

| Table | Key Columns | Sprint |
|---|---|---|
| `stock_entries` | `id`, `variant_id` FK (unique), `quantity`, `reserved_quantity`, `low_stock_threshold`, `row_version` (concurrency) | 6 |

### Schema: `cart`

| Table | Key Columns | Sprint |
|---|---|---|
| `carts` | `id`, `user_id` FK (nullable), `session_id` (nullable, unique), `status`, `created_at`, `updated_at` | 7 |
| `cart_items` | `id`, `cart_id` FK, `variant_id` FK, `quantity`, `added_at`; unique `(cart_id, variant_id)` | 7 |

### Schema: `orders`

| Table | Key Columns | Sprint |
|---|---|---|
| `orders` | `id`, `order_number` (unique), `user_id` FK, `status`, `total` (numeric 12,2), `shipping_address`, `created_at` | 8 |
| `order_items` | `id`, `order_id` FK, `variant_id` FK, `product_name`, `variant_name`, `sku`, `sell_price` (snapshot), `quantity` | 8 |
| `idempotency_requests` | `id`, `key` (unique), `response` (JSONB), `created_at` | 8 |

### Schema: `outbox`

| Table | Key Columns | Sprint |
|---|---|---|
| `outbox_messages` | `id` (UUID PK), `aggregate_id`, `event_type`, `payload` (JSONB), `occurred_on`, `processed_on` (nullable), `retry_count`, `error`, `dead_letter` | 3 (wired) |

---

## Cross-Domain Contracts

These are the **interfaces between domains**. Defined in `Application/Common/Interfaces/` and implemented in `Infrastructure/`.

### External Interfaces

| Interface | Implemented By | Used By |
|---|---|---|
| `IJwtService` | `JwtService` (Infrastructure.Auth) | Auth handlers |
| `IPasswordService` | `PasswordService` (Infrastructure.Auth) | Auth handlers |
| `IImageService` | `ImageKitService` (Infrastructure.Images) | Product/Variant handlers |
| `IElasticsearchService` | `ElasticsearchService` (Infrastructure.Search) | Search handlers |
| `ICacheService` | `CacheService` (Infrastructure.Caching) | Read query handlers |

### Domain Events (In-Process via Outbox → MediatR)

| Event | Published By | Handled By | Sprint Added |
|---|---|---|---|
| `ProductCreatedEvent` | Catalog | Search (index), Cache (warm) | Pub: 3, Sub: 9/10 |
| `ProductUpdatedEvent` | Catalog | Search (re-index), Cache (invalidate) | Pub: 3, Sub: 9/10 |
| `ProductPriceChangedEvent` | Pricing | Search (re-index), Cache (invalidate) | Pub: 5, Sub: 9/10 |
| `OrderPlacedEvent` | Orders | Inventory (reserve stock) | Pub: 8, Sub: 8 |

---

## API Design

### Public Endpoints (No Auth)

| Method | Path | Sprint | Description |
|---|---|---|---|
| GET | `/api/categories` | 2 | Category tree |
| GET | `/api/categories/{slug}` | 2 | Single category with breadcrumb |
| GET | `/api/products` | 3 | Paginated product list |
| GET | `/api/products/{id}` | 3 | Basic product info |
| GET | `/api/products/{slug}` | 3 | Product by slug |
| GET | `/api/products/{id}/detail` | 4 | Full detail: attrs + variants + prices + images |
| GET | `/api/search` | 9 | Full-text search |
| GET | `/api/search/suggest` | 9 | Autocomplete |
| POST | `/api/authregister` | 1 | Customer registration |
| POST | `/api/authlogin` | 1 | Login |
| POST | `/api/authrefresh` | 1 | Refresh token |
| GET | `/sitemap.xml` | 10 | XML sitemap |
| GET | `/health` | 11 | Liveness |
| GET | `/health/ready` | 11 | Readiness |

### Authenticated Endpoints (Customer)

| Method | Path | Sprint | Description |
|---|---|---|---|
| POST | `/api/auth/logout` | 1 | Logout |
| GET | `/api/authme` | 1 | Current user |
| GET | `/api/cart` | 7 | Get cart |
| POST | `/api/cart/items` | 7 | Add to cart |
| PATCH | `/api/cart/items/{variantId}` | 7 | Update quantity |
| DELETE | `/api/cart/items/{variantId}` | 7 | Remove from cart |
| POST | `/api/cartmerge` | 7 | Merge guest cart |
| POST | `/api/orders` | 8 | Place order (Idempotency-Key required) |
| GET | `/api/orders` | 8 | Order history |
| GET | `/api/orders/{id}` | 8 | Order detail |

### Admin Endpoints (RequirePermission)

| Method | Path | Sprint | Description |
|---|---|---|---|
| POST | `/api/admin/categories` | 2 | Create category |
| PATCH | `/api/admin/categories/{id}` | 2 | Update category |
| DELETE | `/api/admin/categories/{id}` | 2 | Delete category |
| POST | `/api/admin/products` | 3 | Create product |
| PATCH | `/api/admin/products/{id}` | 3 | Update product |
| DELETE | `/api/admin/products/{id}` | 3 | Delete product |
| POST | `/api/admin/products/{id}/variants` | 3 | Add variant |
| PATCH | `/api/admin/products/{id}/variants/{vid}` | 3 | Update variant |
| DELETE | `/api/admin/products/{id}/variants/{vid}` | 3 | Delete variant |
| POST | `/api/admin/categories/{id}/attributes` | 4 | Define attribute |
| POST | `/api/admin/attributes/{id}/options` | 4 | Add option |
| POST | `/api/admin/products/{id}/attributes` | 4 | Set product attribute |
| POST | `/api/admin/variants/{id}/attributes` | 4 | Set variant attribute |
| POST | `/api/admin/products/{id}/variants/{vid}/price` | 5 | Set initial price |
| PATCH | `/api/admin/products/{id}/variants/{vid}/price` | 5 | Update price |
| GET | `/api/admin/products/{id}/variants/{vid}/price/history` | 5 | Price history |
| GET | `/api/admin/inventory` | 6 | Stock list |
| PATCH | `/api/admin/inventory/{variantId}/adjust` | 6 | Adjust stock |
| GET | `/api/admin/inventory/low-stock` | 6 | Low stock report |
| GET | `/api/admin/orders` | 8 | All orders |
| GET | `/api/admin/stats` | 11 | Dashboard stats |
| GET | `/api/admin/stats/top-products` | 11 | Top viewed products |
| GET | `/api/admin/stats/top-searches` | 11 | Top search terms |
| POST | `/api/admin/upload-image` | 3 | Upload to ImageKit |

---

## Non-Functional Requirements

| Requirement | Target | How |
|---|---|---|
| Response time | p95 < 300ms (product detail), < 100ms (cached) | FusionCache + compiled queries |
| Throughput | 1000 concurrent users | Connection pooling, split queries, indexed FKs |
| Search latency | p95 < 200ms | Elasticsearch optimized mapping |
| Cache hit ratio | > 80% | FusionCache with Redis + memory + smart invalidation |
| Availability | 99.9% | Health checks, graceful shutdown, retry policies |
| Cold start | < 3s | Trimmed containers, AOT when ready |
| Outbox delivery | < 5s typical, infinite retry | Quartz.NET 5s poll, exponential backoff, dead-letter |

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| EAV query performance at product scale | Medium | High | Compiled queries, selective indexes, consider JSONB for simple attr lookups in future |
| Outbox event loss on crash before COMMIT | Low | High | Outbox written in same transaction as entity save — if save fails, event never written |
| Outbox duplicate delivery | Medium | Low | All handlers are idempotent — ES re-index overwrites, cache set overwrites |
| Migration drift between local and CI | Low | Medium | CI checks EF snapshot against committed migration, staging DB auto-validates |
| PostgreSQL connection pool exhaustion | Low | Medium | `IDbContextFactory` for long-lived scenarios, dispose pattern enforced |

---

## Architecture Decision Records (Topics to Document)

1. Modular monolith over microservices — why one solution, folder boundaries
2. Two-Repository pattern — `IRepository<T>` (tracked writes) vs `IReadRepository<T>` (`AsNoTracking()` reads), same `Repository<T>` class implements both
3. Result pattern over exceptions — FluentResults for domain errors
4. Outbox + MediatR over direct event publishing — reliability + at-least-once
5. PostgreSQL schemas over separate databases — operational simplicity
6. Permission-based auth over role-based — fine-grained `Permission` enum (`AccessUsers`, `MutateUsers`) assigned to users via join table, checked via custom middleware, replaces rigid role concept
7. Mapster over AutoMapper — performance + convention-based mapping
8. FusionCache over raw IDistributedCache — stampede protection + failback
9. EAV for attributes — flexibility over query simplicity, mitigations at scale
10. EF Core over Dapper — compiled queries + AsNoTracking close the gap
