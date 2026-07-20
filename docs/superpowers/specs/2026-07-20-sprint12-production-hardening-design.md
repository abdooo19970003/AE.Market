# Sprint 12 — Production Hardening Design

## Overview

Final sprint: production-ready Docker images, CI/CD pipeline, security hardening, documentation, and load testing. This sprint produces a deployable, auditable, and observable artifact.

## Structure

Sequential by area: Docker → CI/CD → Security → Docs → Load Testing. Each area builds on the previous one and can be verified incrementally.

---

## 1. Docker Optimization

### Current State

VS-generated Dockerfile with 3 stages (base, build, publish, final). Uses `mcr.microsoft.com/dotnet/aspnet:10.0` (full Debian-based image, ~210MB). Already has `USER $APP_UID`. Dev compose works but has no production counterpart.

### Changes

**Dockerfile** (`AE.Market.API/Dockerfile`):
- Switch base to `mcr.microsoft.com/dotnet/aspnet:10.0-chiseled` (~100MB, minimal attack surface)
- Optimize restore: copy only `.csproj` files before `dotnet restore`, then copy source
- Keep multi-stage: restore → build → publish → final
- Final stage: copy published output, set `ENTRYPOINT`

**docker-compose.prod.yml** (new file):
- Init container: uses `mcr.microsoft.com/dotnet/sdk:10.0` image, installs `dotnet-ef` tool, runs `dotnet ef database update` before API starts (dependency: `ae.market.api`)
- API service: resource limits (CPU: 1.0, memory: 512MB), health check, secrets via `.env` file
- PostgreSQL: resource limits, persistent volume
- Redis: resource limits, persistent volume
- No reverse proxy (left to deployment platform: K8s, Cloud Run, etc.)

**Files:**
- Modify: `AE.Market.API/Dockerfile`
- Create: `docker-compose.prod.yml`
- Create: `.env.example` (template for secrets)

---

## 2. CI/CD Pipeline

### Design

GitHub Actions workflow (`.github/workflows/ci.yml`):

**Triggers:**
- Push to `main`
- Pull requests targeting `main`

**Jobs:**

1. **build-and-test** (runs on all triggers):
   - Checkout code
   - Setup .NET 10.0
   - Cache NuGet packages (`~/.nuget/packages`)
   - `dotnet restore`
   - `dotnet build --no-restore`
   - `dotnet test` (domain + architecture tests)

2. **integration-tests** (runs on all triggers, depends on build-and-test):
   - Starts PostgreSQL, Redis, Elasticsearch via Docker Compose
   - Runs integration tests
   - Tears down containers

3. **docker-push** (runs on `main` push only, depends on build-and-test):
   - Build Docker image
   - Push to `ghcr.io/{owner}/{repo}:latest` and `ghcr.io/{owner}/{repo}:{sha}`
   - Uses `GITHUB_TOKEN` for GHCR auth

**Files:**
- Create: `.github/workflows/ci.yml`

---

## 3. Security

### 3.1 SecurityHeadersMiddleware

Custom middleware added to the pipeline early (before routing):

| Header | Value |
|--------|-------|
| `Content-Security-Policy` | `default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none'` |
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` |
| `X-Frame-Options` | `DENY` |
| `X-Content-Type-Options` | `nosniff` |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()` |

**Files:**
- Create: `AE.Market.API/Middleware/SecurityHeadersMiddleware.cs`
- Modify: `AE.Market.API/Program.cs` (register middleware)

### 3.2 CORS Policy

Configurable via `appsettings.json`:

```json
{
  "Cors": {
    "AdminOrigins": ["https://admin.aemarket.com"],
    "PublicOrigins": ["https://aemarket.com", "https://www.aemarket.com"],
    "DefaultOrigins": []
  }
}
```

Three policies:
- `AdminPolicy` — full access (GET, POST, PUT, DELETE) for admin origins
- `PublicPolicy` — read-only (GET) for public origins
- `DefaultPolicy` — same-origin only (no CORS headers)

**Files:**
- Create: `AE.Market.API/Configuration/CorsOptions.cs`
- Modify: `AE.Market.API/Program.cs` (configure CORS)
- Modify: `appsettings.json` / `appsettings.Development.json` (add Cors section)

### 3.3 Rate Limiting

Using `Microsoft.AspNetCore.RateLimiting` (built-in):

| Scope | Limit | Window | Key |
|-------|-------|--------|-----|
| Global | 100 requests | 1 minute | Per IP |
| Auth endpoints (`/api/auth/*`) | 10 requests | 1 minute | Per IP |
| Admin endpoints (`/api/admin/*`) | 60 requests | 1 minute | Per user ID (from JWT) or IP |

**Files:**
- Modify: `AE.Market.API/Program.cs` (configure rate limiting)
- Modify: `AE.Market.API/Program.cs` (add `app.UseRateLimiter()`)

### 3.4 JWT Rotation Enforcement

Enhance existing JWT middleware:
- Check `exp` claim — if token expires within 5 minutes, return 401 with `X-Token-Expiry` header
- Client should use refresh token to get new access token
- Does NOT reject tokens that are already expired (that's already handled)

**Files:**
- Modify: `AE.Market.API/Middleware/CurrentUserMiddleware.cs` (add early expiry check before resolving current user)

---

## 4. Documentation

### 4.1 Architecture Decision Records

Create `docs/decisions.md` with 10 ADRs:

| # | Decision | Status | Summary |
|---|----------|--------|---------|
| 1 | Result Pattern | Accepted | Custom `Result<T>` over exceptions for domain flow control |
| 2 | Modular Monolith | Accepted | Folder-based feature boundaries, single deployable unit |
| 3 | Outbox Pattern | Accepted | Domain events via EF Core SaveChanges interceptor + Quartz processor |
| 4 | CQRS with MediatR | Accepted | Commands/Queries separated, pipeline behaviors for cross-cutting |
| 5 | FusionCache | Accepted | In-memory + distributed caching with `ICachedQuery` marker interface |
| 6 | Elasticsearch for Search | Accepted | Search as read proxy, not CQRS source of truth |
| 7 | Mapster | Accepted | Mapping via `IMapper` adapter over AutoMapper |
| 8 | FluentValidation | Accepted | Request validation in MediatR pipeline via `ValidationBehavior` |
| 9 | Custom JWT Auth | Accepted | Permission-based auth via `[HasPermission]` attribute, no ASP.NET Identity |
| 10 | EF Core over Dapper | Accepted | Code-first migrations, specification pattern, no raw SQL except analytics |

**Files:**
- Create: `docs/decisions.md`

### 4.2 Runbook

Create `docs/runbook.md`:

- **Startup sequence:** Docker Compose services and dependency order
- **Health checks:** `/health` (liveness), `/health/ready` (readiness) — what each verifies
- **Common failures:** Database connection, Redis connection, Elasticsearch down, JWT secret missing
- **Database migrations:** `dotnet ef database update` commands, rollback via `dotnet ef database update <PreviousMigration>`
- **Logging:** Seq UI at `:8082`, log levels, structured logging fields
- **Rollback:** Docker image tag rollback, database migration rollback procedure

**Files:**
- Create: `docs/runbook.md`

### 4.3 API Documentation

Verify OpenAPI/Scalar completeness:
- Ensure all controllers have `[ProducesResponseType]` attributes
- Ensure all DTOs have XML doc comments for schema descriptions
- Verify Scalar UI renders correctly at `/scalar`

**Files:**
- Modify: `AE.Market.API/Controllers/*.cs` (add `[ProducesResponseType]` where missing)

---

## 5. Load Testing

### k6 Scripts

Create `tests/load/` directory with:

**Scripts:**
- `scripts/product-list.js` — GET `/api/products` with pagination, 200 concurrent users, ramp-up 30s, sustain 2min
- `scripts/product-detail.js` — GET `/api/products/{id}` with random IDs, 200 concurrent users
- `scripts/search.js` — GET `/api/search?q=...` with random queries, 100 concurrent users
- `scripts/order-placement.js` — POST `/api/orders` with auth, 50 concurrent users

**Thresholds:**
- p95 < 500ms (all endpoints)
- p99 < 1000ms (all endpoints)
- Error rate < 1%

**Configuration:**
- `thresholds.yml` — shared threshold config
- `README.md` — prerequisites (k6 installed), how to run, interpreting results

**Files:**
- Create: `tests/load/scripts/product-list.js`
- Create: `tests/load/scripts/product-detail.js`
- Create: `tests/load/scripts/search.js`
- Create: `tests/load/scripts/order-placement.js`
- Create: `tests/load/thresholds.yml`
- Create: `tests/load/README.md`

---

## Execution Order

1. Docker optimization (Dockerfile + prod compose)
2. CI/CD pipeline (GitHub Actions)
3. Security (headers + CORS + rate limiting + JWT rotation)
4. Documentation (ADRs + runbook + API docs)
5. Load testing (k6 scripts)

## Verification

After each area:
- Docker: `docker compose -f docker-compose.prod.yml config` validates
- CI/CD: workflow syntax valid, manual trigger test
- Security: unit tests for middleware, manual verification via curl headers
- Docs: reviewed for completeness
- Load tests: k6 runs successfully (even if against dev environment)
