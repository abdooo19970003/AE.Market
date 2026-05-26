<!-- BEGIN:dotnet-clean-architecture -->
# AeMarket .NET Backend — Agent Rules

This project is a **production-ready e-commerce backend** built with ASP.NET Core 10, EF Core 10, PostgreSQL, Elasticsearch, and Redis following **Clean Architecture + CQRS + Modular Monolith**.

## Architecture Layers

```
AeMarket.Api               — Controllers, Middleware, Program.cs
AeMarket.Application       — Commands, Queries, DTOs, FluentValidation, MediatR behaviors
AeMarket.Domain            — Entities, Value Objects, Enums, Domain Events
AeMarket.Infrastructure    — EF Core (Write/Read DbContext), Elasticsearch, Redis, External Services
tests/                     — Unit + Integration tests (Testcontainers)
```

5 projects, 1 solution. Domain boundaries are by **folder convention**, not project references.

## Critical Rules

### Two-DbContext Pattern
- **WriteDbContext** — default tracking, used for commands and EF migrations
- **ReadDbContext** — `QueryTrackingBehavior.NoTrackingWithIdentityResolution`, used for all queries
- Both share entity configurations via `ApplyConfigurationsFromAssembly`
- Never use `ReadDbContext` for writes

### CQRS with MediatR
- Commands mutate state, Queries return data — never mix
- Every command/query gets a FluentValidation validator
- Pipeline behaviours in order: **Validation → Logging → Transaction** (commands only)
- Handlers are thin — orchestrate, don't contain business logic

### Result Pattern (FluentResults)
- **All handlers return `Result<T>` or `Result`** — never throw for control flow
- `Result.Fail(NotFoundError)` → Controller maps to 404
- `Result.Fail(ValidationError)` → 400/422
- `Result.Fail(ConflictError)` → 409
- Exceptions only for programming errors, never domain logic

### Domain Model
- Rich domain model with value objects (`Money`, `Slug`, `Sku`, `Email`, `PasswordHash`)
- Override `Equals` / `GetHashCode` on all value objects
- Domain events stored in `BaseEntity._domainEvents` list
- Events cleared after `SaveChangesAsync` via interceptor → outbox

### Domain Events & Outbox
- Entities call `AddDomainEvent(new SomeEvent(...))` inside command methods
- `SaveChangesInterceptor` collects events before save, writes to `outbox_messages` table after save
- `OutboxProcessorJob` (Quartz.NET, 5s poll) publishes to MediatR
- Retry with exponential backoff, dead-letter after 10 failures
- Handlers are idempotent — duplicate delivery is safe

### EF Core Conventions
- Use `SplitQuery` for includes that could cause cartesian explosion
- Prefer compiled queries for hot-path reads
- **Add explicit indexes on all FK columns** (EF doesn't auto-index FKs)
- `numeric` columns map to `decimal` in C# with `.HasPrecision(12, 2)`
- All entity configurations in dedicated `IEntityTypeConfiguration<T>` files
- Partial unique indexes where needed (e.g. `WHERE is_active = true` on prices)

### Coding Conventions
- File-scoped namespaces (`namespace X.Y;`)
- Primary constructors for DI dependencies
- `Async` suffix on async methods
- `CancellationToken` as last parameter, pass to all EF/MediatR calls
- Mapster for entity → DTO mapping (not AutoMapper)
- `FluentValidation` for input validation (not data annotations)

### Database Schemas
- `auth` — users, refresh_tokens, user_profiles
- `catalog` — categories, products, variants, images, attributes
- `pricing` — variant_prices, price_history
- `inventory` — stock_entries
- `cart` — carts, cart_items
- `orders` — orders, order_items, idempotency_requests
- `outbox` — outbox_messages

### Search (Elasticsearch)
- Index name: `products-v1`
- Sync via MediatR notification handlers on domain events (processed through outbox)
- Full re-index via Quartz.NET nightly job
- Faceted search with dynamic attribute filters

### Caching (FusionCache)
- Always use FusionCache, not raw `IDistributedCache`
- Cache key convention: `{entity}:{type}:{id}` (e.g. `product:detail:42`)
- Invalidate cache entries via notification handlers (processed through outbox)
- Stampede protection enabled by default

### Idempotency
- `POST /api/orders` requires `Idempotency-Key` header
- Keys stored in `orders.idempotency_requests` table
- Duplicate key returns cached response — never creates duplicate order

## Sprint Reference

The full implementation plan is in `BACKEND_PLAN.md` — 12 sprints + 4 bonus sprints. Each sprint ships runnable API endpoints with tests. Always consult it before starting work on a new sprint.

## Technology Stack
- ASP.NET Core 10, EF Core 10, PostgreSQL 17
- MediatR + FluentValidation + FluentResults
- Mapster (mapping)
- Elasticsearch 8 (search)
- FusionCache + Redis (caching)
- Serilog + OpenTelemetry (observability)
- Quartz.NET (scheduled jobs + outbox processor)
- JWT custom auth (not ASP.NET Identity)
- Testcontainers (integration tests)
- Docker Compose (dev + prod environments)
<!-- END:dotnet-clean-architecture -->
