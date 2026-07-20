# Architecture Decision Records

## ADR-001: Result Pattern

**Status:** Accepted

**Context:** Domain operations can fail (not found, validation error, conflict). We need a consistent way to propagate errors without exceptions.

**Decision:** Use custom `Result<T>` / `Result` types with error codes. Handlers return `Result<T>`, never throw for control flow. Controllers use `.ToActionResult()` extension.

**Consequences:**
- Explicit error handling at every layer
- No hidden exception propagation
- Error codes follow convention: `Application.Validation` → 400, `Application.NotFound` → 404, `Application.Conflict` → 409

---

## ADR-002: Modular Monolith

**Status:** Accepted

**Context:** Need clear feature boundaries without the complexity of separate projects/deployments.

**Decision:** Folder-based feature boundaries under `Application/Features/{Feature}/`. Each feature has `Commands/`, `Queries/`, `DTOs/`, `Specs/`, `CacheKeys`. Domain boundaries by folder convention in `Domain/Aggregates/`.

**Consequences:**
- Single deployable unit
- Clear feature organization
- Easy to extract to microservices later if needed
- Architecture tests enforce folder structure

---

## ADR-003: Outbox Pattern

**Status:** Accepted

**Context:** Need reliable domain event publishing without distributed transactions.

**Decision:** EF Core `SaveChangesInterceptor` writes domain events to `outbox.outbox_messages` table. `OutboxProcessorJob` (Quartz.NET) polls every 100 seconds, publishes via MediatR, dead-letters after 10 retries.

**Consequences:**
- At-least-once delivery guarantee
- No distributed transactions
- Events survive application crashes
- Raw SQL `FOR UPDATE SKIP LOCKED` for concurrent processing

---

## ADR-004: CQRS with MediatR

**Status:** Accepted

**Context:** Need separation of read and write operations with pipeline behaviors for cross-cutting concerns.

**Decision:** Commands implement `ICommand<T>` / `ICommand`, queries implement `IBaseQuery<T>`. MediatR dispatches through pipeline: ExceptionHandler → Logging → Caching → Validation → Transaction.

**Consequences:**
- Clear separation of concerns
- Pipeline behaviors for cross-cutting logic
- Easy to add new behaviors without modifying handlers

---

## ADR-005: FusionCache

**Status:** Accepted

**Context:** Need in-memory + distributed caching with stampede prevention.

**Decision:** FusionCache via `ICacheService` interface. Queries implement `ICachedQuery` marker interface with `CacheKey`, `AbsoluteExpiration`, `SlidingExpiration`. `CachingBehavior` auto-caches queries.

**Consequences:**
- Two-level caching (L1 memory + L2 Redis)
- Stampede prevention via FusionCache built-in
- Simple cache invalidation via exact key removal

---

## ADR-006: Elasticsearch for Search

**Status:** Accepted

**Context:** Need full-text search with filters, facets, and relevance scoring.

**Decision:** Elasticsearch as read proxy. Search index updated via domain event handlers. Queries go directly to ES, not through CQRS. Search feature excluded from CQRS architecture tests.

**Consequences:**
- Fast search queries
- Eventually consistent with write model
- Separate read model for search

---

## ADR-007: Mapster

**Status:** Accepted

**Context:** Need object mapping between domain entities, DTOs, and API contracts.

**Decision:** Mapster behind `IMapper` / `AppMapper` wrapper. Configured globally in `MappingConfig.cs`. Inject `IMapper`, do not call `Adapt<>()` directly.

**Consequences:**
- Single mapping configuration
- Type-safe mapping
- Easy to test (can mock IMapper)

---

## ADR-008: FluentValidation

**Status:** Accepted

**Context:** Need request validation before handler execution.

**Decision:** FluentValidation validators in MediatR pipeline via `ValidationBehavior`. Validators are `public sealed class`, one per command.

**Consequences:**
- Validation before handler runs
- Consistent error responses
- Easy to test validation rules independently

---

## ADR-009: Custom JWT Auth

**Status:** Accepted

**Context:** Need permission-based authentication without ASP.NET Identity complexity.

**Decision:** Custom JWT implementation. Permission-based via `[HasPermission(Permission.X)]` attribute + `PermissionBasedAuthFilter`. `ICurrentUser` resolves current user from JWT claims.

**Consequences:**
- Full control over token generation/validation
- Permission-based authorization
- No Identity framework overhead

---

## ADR-010: EF Core over Dapper

**Status:** Accepted

**Context:** Need data access with migrations, change tracking, and specification pattern.

**Decision:** EF Core with code-first migrations. `SpecificationEvaluator<T>` for read queries with `AsNoTracking()`. No raw SQL except analytics queries.

**Consequences:**
- Type-safe queries
- Automatic change tracking
- Migration support
- Specification pattern for complex queries
