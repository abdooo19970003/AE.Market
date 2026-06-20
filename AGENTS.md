# AeMarket .NET Backend — Agent Rules

## Architecture

```
AE.Market.slnx
├── AE.Market.API/             — Controllers, Middleware, Program.cs, Dockerfile
├── Application/               — Commands, Queries, DTOs, FluentValidation, MediatR behaviors
├── Domain/                    — Entities, Value Objects, Enums, Domain Events (no deps beyond .NET)
├── AE.Market.Infrastructure/  — EF Core, Redis/FusionCache, JWT, Quartz
└── tests/
    ├── AE.Market.Domain.Tests/           — xUnit + FluentAssertions (no Testcontainers)
    ├── AE.Market.ArchitectureTests/      — NetArchTest.Rules
    └── AE.Market.Integration.Tests/      — Testcontainers (PostgreSQL + Redis)
```

6 projects. Domain boundaries by **folder convention** (not separate projects). Currently only **Auth** aggregate is implemented (users, profiles, permissions, refresh tokens). Catalog/Pricing/Inventory/Cart/Orders are planned but absent.

## Critical Conventions

### Result Pattern (Custom, NOT FluentResults)
- `Domain.Common.Result` / `Domain.Common.Result<T>` — custom implementation, no NuGet package
- Handlers return `Result<T>` or `Result`, never throw for control flow
- Controller uses `result.ToActionResult()` / `.ToCreatedActionResult()` / `.ToNotFoundActionResult()` from `API/Helpers/ResultMapper.cs`
- Error code prefix convention: `Application.Validation` → 400, `Application.NotFound` → 404, `Application.Conflict` → 409, `Application.*` → 500

### CQRS Markers (in `Application.Common.Interfaces`)
- `ICommand`, `ICommand<TResponse>` — extend `IBaseCommand`, return `Result`/`Result<T>`
- `IBaseQuery<TResponse>` — returns `Result<TResponse>`
- Commands use `IRepository<T>`, queries use `IReadRepository<T>` (same class behind both, but read methods use `AsNoTracking()`)
- `TransactionBehavior` wraps only `IBaseCommand` handlers

### Pipeline Order
ExceptionHandler → Logging → Caching → Validation → Transaction
Registered in `Application/DependencyInjection.cs:17-21`.

### Caching
- Marker interface `ICachedQuery` on query records (fields: `CacheKey`, `AbsoluteExpiration`, `SlidingExpiration`)
- `CachingBehavior` auto-caches queries that implement `ICachedQuery`
- FusionCache via `ICacheService` (not raw `IDistributedCache`)

### Domain Events & Outbox
- Entities call `AddDomainEvent(...)` in static factory/command methods
- `DomainEventDispatcher` (SaveChangesInterceptor) writes to `outbox.outbox_messages` table
- `OutboxProcessorJob` (Quartz.NET) polls every **100 seconds** (not 5), dead-letter after 10 retries
- Post-save: interceptor calls `ClearDomainEvents()`
- Raw SQL `FOR UPDATE SKIP LOCKED` for message consumption

### EF Core
- `AppDbContext` uses `ApplyConfigurationsFromAssembly` — all configs in `IEntityTypeConfiguration<T>` files under `Persistence/Configurations/`
- All read queries go through `SpecificationEvaluator<T>` with `AsNoTracking()`
- Dev drops & recreates DB on startup: `Program.cs:64-74`
- No compiled queries yet, no explicit FK indexes yet (both aspirational)

### Mapping
- Mapster behind `IMapper` / `AppMapper` wrapper (in `Application.Common.Mapping`)
- Configured globally in `MappingConfig.cs` via `TypeAdapterConfig.GlobalSettings`
- Inject `IMapper`, do not call `Adapt<>()` directly

### Authentication
- Custom JWT (not ASP.NET Identity)
- Permission-based via `[HasPermission(Permission.X)]` attribute + `PermissionBasedAuthFilter`
- Current user resolved via `ICurrentUser` / `CurrentUserService`

### Controllers
- Primary constructor with `IMediator mediator`, `sealed class`
- `[Route("api/[controller]")]`, `[ApiController]`
- Handlers called with `await mediator.Send(cmd)`, result piped through `result.ToActionResult()`

### Coding Style
- File-scoped namespaces (`namespace X.Y;`)
- Primary constructors for DI
- `Async` suffix on async methods, `CancellationToken` as last param
- Value objects are `record` types implementing `IValueObject` (or standalone `record`)

## Commands

```powershell
# Build
dotnet build .\AE.Market.slnx

# Run domain tests only (fast, no infra)
dotnet test .\tests\AE.Market.Domain.Tests\AE.Market.Domain.Tests.csproj

# Run architecture tests
dotnet test .\AE.Market.ArchitectureTests\AE.Market.ArchitectureTests.csproj

# Run all tests
dotnet test .\AE.Market.slnx

# Dev — starts postgres, redis, seq via Docker Compose + API
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Or via VS: F5 uses launchSettings.json (Docker Compose profile)
```

## Known Gaps (from BACKEND_PLAN.md, not yet implemented)
- **No CI workflow** yet (`.github/workflows/` is empty)
- **No Elasticsearch** — no packages, no indices, no queries
- **Outbox interval**: 100s (not 5s as documented in plan)
- Only **Auth** feature has code; Catalog, Pricing, etc. are stubs at most

## Dev Environment
- Docker Compose: postgres:16-alpine, redis, datalust/seq
- Connection strings in `appsettings.Development.json` (gitignored):
  - `Host=postgres;Port=5432;Database=marketDb;Username=postgres;Password=password`
  - `redis:6379,password=password`
- JWT dev secret is read from environment variable `Jwt__Secret` or `appsettings.Development.json`
- API docs at `/scalar` (not Swagger)
- Seq UI at `http://localhost:8082`

## Seed Users (created on first startup in Development mode, drop DB to re-seed)

| Email | Password | Permissions |
|---|---|---|
| `admin@aemarket.com` | `Admin@12345` | AccessUsers, MutateUsers |
| `client@aemarket.com` | `Client@12345` | (none) |

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, invoke the `skill` tool with `skill: "graphify"` before doing anything else.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
