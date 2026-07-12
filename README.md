# AE.Market

Production-ready e-commerce backend built with **Clean Architecture**, **CQRS**, and **DDD** on .NET.

## Architecture

Modular monolith — 4 projects, 1 solution. Domain boundaries by **folder convention** (not separate services).

```
AE.Market.slnx
├── Domain/                        — Entities, VOs, enums, domain events (zero dependencies)
├── Application/                   — Commands, Queries, DTOs, FluentValidation, MediatR behaviors
├── AE.Market.Infrastructure/      — EF Core, Redis/FusionCache, JWT, Quartz outbox
├── AE.Market.API/                 — Controllers, middleware, Program.cs
└── tests/
    ├── AE.Market.Domain.Tests/
    ├── AE.Market.ArchitectureTests/
    └── AE.Market.Integration.Tests/ — Testcontainers (PostgreSQL + Redis)
```

## Stack

| Category | Choice |
|---|---|
| Runtime | ASP.NET Core |
| ORM | EF Core + Npgsql |
| CQRS | MediatR + FluentValidation |
| Mapping | Mapster |
| Database | PostgreSQL (schemas: `auth`, `catalog`, `outbox`) |
| Cache | FusionCache + Redis (distributed) + Memory (L1) |
| Auth | Custom JWT with permission-based authorization |
| Outbox | EF Core interceptor + Quartz.NET polling |
| Jobs | Quartz.NET |
| Logging | Serilog → Console + Seq |

## Implemented Features

### Auth
- User registration, login, JWT access/refresh tokens, logout
- Permission-based authorization (`[HasPermission(Permission.X)]`)
- User profile management
- Refresh token rotation with reuse detection

### Catalog
- **Categories** — hierarchical (self-referencing parent), tree queries
- **Brands** — product brand management
- **Products** — full CRUD with variants, images, attributes
- **Variants** — SKU management, pricing, stock tracking
- **Attributes (EAV)** — dynamic attributes per category with input types (text, number, select, multi-select, boolean)
- **Group Units / Units** — unit of measure management
- **Product Tax Codes** — tax code configuration
- **Tags** — product tagging

### Cross-cutting
- Result pattern (custom, not FluentResults) — no exceptions for control flow
- Domain events with outbox pattern (reliable at-least-once delivery)
- FusionCache with stampede protection, fail-safe, Redis backplane
- Request logging middleware
- Global exception handler → Problem Details RFC
- Seed data for development

## Getting Started

### Prerequisites
- Docker Desktop
- .NET SDK

### Run (Development)
```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
dotnet run --project AE.Market.API
```

The dev environment starts PostgreSQL, Redis, and Seq via Docker Compose. The API auto-creates and seeds the database on startup.

### API Documentation
Available at `/scalar` in development mode.

### Seed Users

| Email | Password | Permissions |
|---|---|---|
| `admin@aemarket.com` | `Admin@12345` | AccessUsers, MutateUsers |
| `client@aemarket.com` | `Client@12345` | (none) |

## Testing

```bash
# Domain tests (fast, no infrastructure)
dotnet test tests/AE.Market.Domain.Tests

# Architecture tests
dotnet test AE.Market.ArchitectureTests

# All tests
dotnet test AE.Market.slnx
```

Integration tests use Testcontainers (PostgreSQL + Redis).

## Key Conventions

- **Result pattern**: All handlers return `Result<T>` / `Result`. Controllers map via `result.ToActionResult()`.
- **CQRS markers**: `ICommand<T>` / `IBaseQuery<T>` — commands use `IRepository<T>`, queries use `IReadRepository<T>` (both same class, read methods use `AsNoTracking()`).
- **Pipeline**: ExceptionHandler → Logging → Caching → Validation → Transaction.
- **Caching**: Queries implementing `ICachedQuery` auto-cache via FusionCache.
- **Domain events**: Entities call `AddDomainEvent(...)` → interceptor writes to `outbox_messages` → Quartz job polls every 100s.
- **Mapster**: Inject `IMapper`, never call `Adapt<>()` directly.
