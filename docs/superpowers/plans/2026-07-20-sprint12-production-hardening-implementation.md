# Sprint 12 — Production Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produce a production-ready, deployable Docker image with CI/CD, security hardening, documentation, and load testing.

**Architecture:** Sequential by area: Docker → CI/CD → Security (headers, CORS, rate limiting, JWT) → Docs → Load Testing. Each task is self-contained and independently verifiable.

**Tech Stack:** Docker (chiseled images), GitHub Actions, ASP.NET Core middleware, k6

## Global Constraints

- File-scoped namespaces (`namespace X.Y;`)
- Primary constructors for DI
- `Async` suffix on async methods, `CancellationToken` as last param
- Handlers are `internal sealed class`, validators are `public sealed class`
- Controllers are `sealed class` with `[Route("api/[controller]")]`
- Middleware follows `Middlewares/` folder convention (plural), `sealed class` with `InvokeAsync`
- Use `Result<T>` / `Result` pattern — never throw for control flow

---

## Task 1: Docker Optimization

**Files:**
- Modify: `AE.Market.API/Dockerfile`
- Create: `docker-compose.prod.yml`
- Create: `.env.example`

**Interfaces:**
- Consumes: existing `AE.Market.API.csproj` project structure
- Produces: optimized Docker image, production compose file

- [ ] **Step 1: Rewrite Dockerfile with chiseled images**

Replace `AE.Market.API/Dockerfile` with:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0-chiseled AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY ["AE.Market.API/AE.Market.API.csproj", "AE.Market.API/"]
COPY ["Application/AE.Market.Application.csproj", "Application/"]
COPY ["Domain/AE.Market.Domain.csproj", "Domain/"]
COPY ["AE.Market.Infrastructure/AE.Market.Infrastructure.csproj", "AE.Market.Infrastructure/"]
RUN dotnet restore "./AE.Market.API/AE.Market.API.csproj"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
COPY . .
WORKDIR "/src/AE.Market.API"
RUN dotnet build "./AE.Market.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AE.Market.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AE.Market.API.dll"]
```

- [ ] **Step 2: Validate Dockerfile syntax**

Run: `docker build -t aemarket-test -f AE.Market.API/Dockerfile .`
Expected: Build succeeds (or fails only because Docker daemon not running — that's OK)

- [ ] **Step 3: Create docker-compose.prod.yml**

Create `docker-compose.prod.yml`:

```yaml
services:
  ae.market.api:
    image: ghcr.io/${GITHUB_REPOSITORY}:latest
    build:
      context: .
      dockerfile: AE.Market.API/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Database=Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      - ConnectionStrings__redis=redis:6379,password=${REDIS_PASSWORD},abortConnect=false
      - Jwt__Secret=${JWT_SECRET}
      - Jwt__Issuer=${JWT_ISSUER}
      - Jwt__Audience=${JWT_AUDIENCE}
      - Jwt__ExpirationInMinutes=${JWT_EXPIRATION_MINUTES}
      - Elasticsearch__Uri=http://elasticsearch:9200
    depends_on:
      db-migration:
        condition: service_completed_successfully
      redis:
        condition: service_healthy
      elasticsearch:
        condition: service_healthy
    deploy:
      resources:
        limits:
          cpus: "1.0"
          memory: 512M
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 15s
    restart: unless-stopped

  db-migration:
    image: mcr.microsoft.com/dotnet/sdk:10.0
    working_dir: /app
    volumes:
      - ./AE.Market.API:/app
      - ./Application:/app/Application
      - ./Domain:/app/Domain
      - ./AE.Market.Infrastructure:/app/AE.Market.Infrastructure
    environment:
      - ConnectionStrings__Database=Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
    entrypoint: >
      sh -c "
        dotnet tool install --global dotnet-ef --version 10.0.0 &&
        export PATH=$PATH:/root/.dotnet/tools &&
        dotnet restore AE.Market.API/AE.Market.API.csproj &&
        dotnet ef database update --project AE.Market.Infrastructure --startup-project AE.Market.API
      "

  postgres:
    image: postgres:16-alpine
    environment:
      - POSTGRES_USER=${POSTGRES_USER}
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
      - POSTGRES_DB=${POSTGRES_DB}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    deploy:
      resources:
        limits:
          cpus: "1.0"
          memory: 512M
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

  redis:
    image: redis:7-alpine
    command: redis-server --requirepass ${REDIS_PASSWORD} --appendonly yes
    volumes:
      - redis_data:/data
    deploy:
      resources:
        limits:
          cpus: "0.5"
          memory: 256M
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.14.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms256m -Xmx256m"
    volumes:
      - es_data:/usr/share/elasticsearch/data
    deploy:
      resources:
        limits:
          cpus: "1.0"
          memory: 512M
    healthcheck:
      test: ["CMD-SHELL", "curl -f http://localhost:9200/_cluster/health || exit 1"]
      interval: 15s
      timeout: 10s
      retries: 5
    restart: unless-stopped

volumes:
  postgres_data:
  redis_data:
  es_data:
```

- [ ] **Step 4: Create .env.example**

Create `.env.example`:

```bash
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=changeme
POSTGRES_DB=marketDb

# Redis
REDIS_PASSWORD=changeme

# JWT
JWT_SECRET=changeme-use-random-64-char-string
JWT_ISSUER=AE.Market.API
JWT_AUDIENCE=developers
JWT_EXPIRATION_IN_MINUTES=60

# GitHub (for CI/CD)
GITHUB_REPOSITORY=owner/repo
```

- [ ] **Step 5: Validate docker-compose.prod.yml**

Run: `docker compose -f docker-compose.prod.yml config`
Expected: Outputs valid YAML (may warn about missing .env file — that's OK)

- [ ] **Step 6: Commit**

```bash
git add AE.Market.API/Dockerfile docker-compose.prod.yml .env.example
git commit -m "feat(infra): optimize Dockerfile with chiseled images and add prod compose"
```

---

## Task 2: CI/CD Pipeline

**Files:**
- Create: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: existing test projects, Dockerfile
- Produces: GitHub Actions workflow, GHCR image push

- [ ] **Step 1: Create GitHub Actions workflow**

Create `.github/workflows/ci.yml`:

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: "10.0.x"
  REGISTRY: ghcr.io

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release

      - name: Domain Tests
        run: dotnet test tests/AE.Market.Domain.Tests/AE.Market.Domain.Tests.csproj --no-build -c Release --verbosity normal

      - name: Architecture Tests
        run: dotnet test AE.Market.ArchitectureTests/AE.Market.ArchitectureTests.csproj --no-build -c Release --verbosity normal

  integration-tests:
    runs-on: ubuntu-latest
    needs: build-and-test
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_USER: postgres
          POSTGRES_PASSWORD: password
          POSTGRES_DB: marketDb
        ports:
          - 5432:5432
        options: >-
          --health-cmd "pg_isready -U postgres"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

      redis:
        image: redis:7
        ports:
          - 6379:6379
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

      elasticsearch:
        image: docker.elastic.co/elasticsearch/elasticsearch:8.14.0
        env:
          discovery.type: single-node
          xpack.security.enabled: "false"
        ports:
          - 9200:9200
        options: >-
          --health-cmd "curl -f http://localhost:9200/_cluster/health || exit 1"
          --health-interval 15s
          --health-timeout 10s
          --health-retries 5

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release

      - name: Integration Tests
        run: dotnet test tests/AE.Market.Integration.Tests/AE.Market.Integration.Tests.csproj --no-build -c Release --verbosity normal
        env:
          ConnectionStrings__Database: "Host=localhost;Port=5432;Database=marketDb;Username=postgres;Password=password"
          ConnectionStrings__redis: "localhost:6379,password=password,abortConnect=false"
          Elasticsearch__Uri: "http://localhost:9200"
          Jwt__Secret: "TestSecretForCI123456789012345678"
          Jwt__Issuer: "AE.Market.API"
          Jwt__Audience: "developers"
          Jwt__ExpirationInMinutes: "60"
          ASPNETCORE_ENVIRONMENT: "IntegrationTest"

  docker-push:
    runs-on: ubuntu-latest
    needs: build-and-test
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    permissions:
      contents: read
      packages: write
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Log in to GHCR
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ github.repository }}
          tags: |
            type=sha
            type=raw,value=latest

      - name: Build and push
        uses: docker/build-push-action@v5
        with:
          context: .
          file: AE.Market.API/Dockerfile
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
```

- [ ] **Step 2: Validate workflow syntax**

Run: `cat .github/workflows/ci.yml | head -5`
Expected: Valid YAML output

- [ ] **Step 3: Commit**

```bash
mkdir -p .github/workflows
git add .github/workflows/ci.yml
git commit -m "ci: add GitHub Actions workflow with build, test, and Docker push"
```

---

## Task 3: SecurityHeadersMiddleware + CORS

**Files:**
- Create: `AE.Market.API/Middlewares/SecurityHeadersMiddleware.cs`
- Create: `AE.Market.API/Configuration/CorsOptions.cs`
- Modify: `AE.Market.API/Program.cs`
- Modify: `AE.Market.API/appsettings.json`
- Modify: `AE.Market.API/appsettings.Development.json`
- Test: `tests/AE.Market.ArchitectureTests/AE.Market.ArchitectureTests.csproj` (verify arch tests still pass)

**Interfaces:**
- Consumes: existing `Middlewares/` folder pattern, `Program.cs` pipeline
- Produces: security headers on all responses, CORS policies

- [ ] **Step 1: Create SecurityHeadersMiddleware**

Create `AE.Market.API/Middlewares/SecurityHeadersMiddleware.cs`:

```csharp
namespace AE.Market.API.Middlewares;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["Content-Security-Policy"] =
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none'";
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        headers["X-Frame-Options"] = "DENY";
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        await next(context);
    }
}
```

- [ ] **Step 2: Create CorsOptions**

Create `AE.Market.API/Configuration/CorsOptions.cs`:

```csharp
namespace AE.Market.API.Configuration;

public sealed class CorsOptions
{
    public string[] AdminOrigins { get; set; } = [];
    public string[] PublicOrigins { get; set; } = [];
    public string[] DefaultOrigins { get; set; } = [];
}
```

- [ ] **Step 3: Add CORS config to appsettings.json**

Modify `AE.Market.API/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AdminOrigins": [],
    "PublicOrigins": [],
    "DefaultOrigins": []
  }
}
```

- [ ] **Step 4: Add CORS config to appsettings.Development.json**

Modify `AE.Market.API/appsettings.Development.json` (create if missing):

```json
{
  "Cors": {
    "AdminOrigins": ["https://localhost:3000", "http://localhost:3000"],
    "PublicOrigins": ["https://localhost:3001", "http://localhost:3001"],
    "DefaultOrigins": []
  }
}
```

- [ ] **Step 5: Register middleware and CORS in Program.cs**

Modify `AE.Market.API/Program.cs` — add using and registration:

```csharp
// Add these usings at the top
using AE.Market.API.Configuration;
using AE.Market.API.Middlewares;
```

In `CreateWebApplication`, after `builder.Services.AddInfrastructureServices(...)`:

```csharp
// Security: CORS
var corsOptions = new CorsOptions();
builder.Configuration.GetSection("Cors").Bind(corsOptions);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.WithOrigins(corsOptions.AdminOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader());

    options.AddPolicy("PublicPolicy", policy =>
        policy.WithOrigins(corsOptions.PublicOrigins)
              .WithMethods("GET")
              .AllowAnyHeader());

    options.AddPolicy("DefaultPolicy", policy =>
        policy.WithOrigins(corsOptions.DefaultOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader());
});
```

In the middleware pipeline, BEFORE `app.UseAuthentication()`:

```csharp
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("DefaultPolicy");
```

- [ ] **Step 6: Build to verify compilation**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`
Expected: Build succeeds

- [ ] **Step 7: Run arch tests to verify no regressions**

Run: `dotnet test .\AE.Market.ArchitectureTests\AE.Market.ArchitectureTests.csproj --verbosity quiet`
Expected: 55/55 pass

- [ ] **Step 8: Commit**

```bash
git add AE.Market.API/Middlewares/SecurityHeadersMiddleware.cs AE.Market.API/Configuration/CorsOptions.cs AE.Market.API/Program.cs AE.Market.API/appsettings.json AE.Market.API/appsettings.Development.json
git commit -m "feat(security): add SecurityHeadersMiddleware and configurable CORS policy"
```

---

## Task 4: Rate Limiting + JWT Rotation

**Files:**
- Modify: `AE.Market.API/Program.cs`
- Modify: `AE.Market.API/Middlewares/SecurityHeadersMiddleware.cs` (already created in Task 3)

**Interfaces:**
- Consumes: existing auth pipeline, `JwtService`, `CurrentUserMiddleware` pattern
- Produces: rate limiting on all endpoints, JWT early expiry rejection

- [ ] **Step 1: Add rate limiting to Program.cs**

Modify `AE.Market.API/Program.cs` — add using:

```csharp
using System.Threading.RateLimiting;
```

In `CreateWebApplication`, after CORS registration:

```csharp
// Security: Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("admin", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra)
                ? (int)ra.TotalSeconds : 60
        }, ct);
    };
});
```

In the middleware pipeline, AFTER `app.UseAuthorization()` but BEFORE `app.MapControllers()`:

```csharp
app.UseRateLimiter();
```

- [ ] **Step 2: Add rate limit endpoints to auth and admin controllers**

Modify `AE.Market.API/Controllers/AdminController.cs` — add using:

```csharp
using Microsoft.AspNetCore.RateLimiting;
```

Add `[EnableRateLimiting("admin")]` attribute to the class.

Modify any Auth controller — add `[EnableRateLimiting("auth")]` attribute.

- [ ] **Step 3: Add JWT early expiry check to CurrentUserService**

Modify `AE.Market.API/Authentication/CurrentUserService.cs`:

```csharp
using System.Security.Claims;

namespace AE.Market.API.Authentication;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var claim = _user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim is not null ? Guid.Parse(claim) : Guid.Empty;
        }
    }

    public string? Email => _user?.FindFirstValue(ClaimTypes.Email);

    public Permission[] Permissions
    {
        get
        {
            var claim = _user?.FindFirstValue("Permissions");
            if (claim is null) return [];
            return claim
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => Enum.Parse<Permission>(p.Trim()))
                .ToArray();
        }
    }

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Checks if the JWT token is approaching expiry (within 5 minutes).
    /// Returns true if the token should be refreshed.
    /// </summary>
    public bool IsTokenExpiringSoon()
    {
        var expClaim = _user?.FindFirstValue("exp");
        if (expClaim is null) return false;

        if (long.TryParse(expClaim, out var expUnix))
        {
            var expDateTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            return expDateTime - DateTime.UtcNow < TimeSpan.FromMinutes(5);
        }
        return false;
    }
}
```

- [ ] **Step 4: Add IsTokenExpiringSoon to ICurrentUser interface**

Modify `Application/Common/Interfaces/ICurrentUser.cs` — add:

```csharp
bool IsTokenExpiringSoon();
```

- [ ] **Step 5: Add JWT expiry middleware**

Create `AE.Market.API/Middlewares/JwtExpiryMiddleware.cs`:

```csharp
using AE.Market.API.Authentication;
using AE.Market.Application.Common.Interfaces;

namespace AE.Market.API.Middlewares;

public sealed class JwtExpiryMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        if (currentUser.IsAuthenticated && currentUser.IsTokenExpiringSoon())
        {
            context.Response.Headers["X-Token-Expiry"] = "approaching";
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Token expiring soon",
                message = "Please use your refresh token to get a new access token"
            });
            return;
        }

        await next(context);
    }
}
```

In `Program.cs`, register AFTER `app.UseAuthorization()` but BEFORE `app.UseRateLimiter()`:

```csharp
app.UseMiddleware<JwtExpiryMiddleware>();
```

- [ ] **Step 6: Build to verify compilation**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add AE.Market.API/Program.cs AE.Market.API/Controllers/AdminController.cs AE.Market.API/Authentication/CurrentUserService.cs Application/Common/Interfaces/ICurrentUser.cs AE.Market.API/Middlewares/JwtExpiryMiddleware.cs
git commit -m "feat(security): add rate limiting, JWT early expiry check, and expiry middleware"
```

---

## Task 5: Documentation (ADRs + Runbook + API Docs)

**Files:**
- Create: `docs/decisions.md`
- Create: `docs/runbook.md`
- Modify: `AE.Market.API/Controllers/AdminController.cs` (add ProducesResponseType)
- Modify: `AE.Market.API/Controllers/ProductsController.cs` (add ProducesResponseType)
- Modify: other controllers as needed

**Interfaces:**
- Consumes: existing architecture decisions, health check endpoints
- Produces: ADR document, runbook, complete OpenAPI spec

- [ ] **Step 1: Create ADR document**

Create `docs/decisions.md`:

```markdown
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
```

- [ ] **Step 2: Create runbook**

Create `docs/runbook.md`:

```markdown
# AE.Market Runbook

## Startup Sequence

### Development (Docker Compose)

```bash
# Start all services
docker compose up -d

# Or with override (adds Jaeger)
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

Services start in order: postgres → redis → elasticsearch → seq → api

### Production

```bash
# Start with production config
docker compose -f docker-compose.prod.yml --env-file .env up -d
```

Init container runs migrations before API starts.

## Health Checks

| Endpoint | Purpose | Expected Response |
|----------|---------|-------------------|
| `GET /health` | Liveness — is the app running? | 200 OK |
| `GET /health/ready` | Readiness — are dependencies available? | 200 OK or 503 |

### What /health/ready Checks

- PostgreSQL connection
- Redis connection
- Elasticsearch connection

## Common Failure Modes

### Database Connection Failed

**Symptom:** `/health/ready` returns 503, API logs "Connection refused"

**Recovery:**
1. Check PostgreSQL container: `docker compose ps postgres`
2. Check logs: `docker compose logs postgres`
3. Restart: `docker compose restart postgres`
4. If data corruption: `docker compose down -v && docker compose up -d`

### Redis Connection Failed

**Symptom:** Cache misses, FusionCache fallback to database

**Recovery:**
1. Check Redis container: `docker compose ps redis`
2. Check logs: `docker compose logs redis`
3. Restart: `docker compose restart redis`

### Elasticsearch Down

**Symptom:** Search queries fail, `/health/ready` returns 503

**Recovery:**
1. Check ES container: `docker compose ps elasticsearch`
2. Check logs: `docker compose logs elasticsearch`
3. Restart: `docker compose restart elasticsearch`
4. Reindex if needed: `curl -X POST http://localhost:9200/_reindex`

### JWT Secret Missing

**Symptom:** All authenticated requests return 401

**Recovery:**
1. Check environment variable: `Jwt__Secret`
2. Ensure it's set in `.env` or environment
3. Restart API container

## Database Migrations

### Apply Migrations

```bash
# Development (auto-applied on startup)
docker compose up -d

# Production (via init container)
docker compose -f docker-compose.prod.yml --env-file .env up -d

# Manual (if needed)
dotnet ef database update --project AE.Market.Infrastructure --startup-project AE.Market.API
```

### Rollback Migration

```bash
# Rollback to previous migration
dotnet ef database update <PreviousMigrationName> --project AE.Market.Infrastructure --startup-project AE.Market.API

# List migrations
dotnet ef migrations list --project AE.Market.Infrastructure --startup-project AE.Market.API
```

## Logging

### Seq UI

Access: `http://localhost:8082`
Default password: `password` (development only)

### Log Levels

Configure in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Structured Logging Fields

- `CorrelationId` — request trace ID
- `UserId` — authenticated user ID
- `Elapsed` — request duration in ms

## Rollback Procedure

### Application Rollback

```bash
# Pull previous version
docker pull ghcr.io/{owner}/{repo}:{previous-tag}

# Update docker-compose.prod.yml image tag
# Restart
docker compose -f docker-compose.prod.yml --env-file .env up -d
```

### Database Rollback

1. Identify the migration to rollback to
2. Run: `dotnet ef database update {MigrationName}`
3. Verify application starts correctly
4. If issues persist, restore from backup

## Performance Monitoring

### Key Metrics

- Request duration (p50, p95, p99)
- Error rate (4xx, 5xx)
- Cache hit/miss ratio
- Database connection pool usage
- Queue depth (outbox messages)

### Jaeger Tracing

Access: `http://localhost:16686` (development)

View distributed traces for request flows across services.
```

- [ ] **Step 3: Add ProducesResponseType to controllers**

Modify `AE.Market.API/Controllers/AdminController.cs` — add `[ProducesResponseType]` attributes:

```csharp
[HttpGet("stats")]
[HasPermission(Permission.AccessUsers)]
[ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(Error), StatusCodes.Status403Forbidden)]
public async Task<IActionResult> GetStats(CancellationToken ct)
```

Repeat pattern for `GetTopProducts` and `GetTopSearches` endpoints.

- [ ] **Step 4: Verify OpenAPI/Scalar**

Run: `dotnet run --project AE.Market.API`
Open: `http://localhost:8080/scalar`
Expected: All endpoints visible with request/response schemas

- [ ] **Step 5: Commit**

```bash
git add docs/decisions.md docs/runbook.md AE.Market.API/Controllers/AdminController.cs
git commit -m "docs: add ADRs, runbook, and API response type attributes"
```

---

## Task 6: Load Testing (k6)

**Files:**
- Create: `tests/load/scripts/product-list.js`
- Create: `tests/load/scripts/product-detail.js`
- Create: `tests/load/scripts/search.js`
- Create: `tests/load/scripts/order-placement.js`
- Create: `tests/load/thresholds.yml`
- Create: `tests/load/README.md`

**Interfaces:**
- Consumes: running API instance (dev or staging)
- Produces: k6 load test scripts, threshold configuration

- [ ] **Step 1: Create shared thresholds**

Create `tests/load/thresholds.yml`:

```yaml
thresholds:
  http_req_duration:
    - threshold: "p(95)<500"
    - threshold: "p(99)<1000"
  http_req_failed:
    - threshold: "rate<0.01"
```

- [ ] **Step 2: Create product-list test**

Create `tests/load/scripts/product-list.js`:

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

export const options = {
  stages: [
    { duration: '30s', target: 200 },
    { duration: '2m', target: 200 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  const page = Math.floor(Math.random() * 10) + 1;
  const pageSize = 20;

  const res = http.get(`${BASE_URL}/api/products?page=${page}&pageSize=${pageSize}`);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}
```

- [ ] **Step 3: Create product-detail test**

Create `tests/load/scripts/product-detail.js`:

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';
import { SharedArray } from 'k6/data';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

// In real usage, fetch product IDs from the API or use a predefined list
const productIds = new SharedArray('products', function () {
  return ['placeholder-id-1', 'placeholder-id-2', 'placeholder-id-3'];
});

export const options = {
  stages: [
    { duration: '30s', target: 200 },
    { duration: '2m', target: 200 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  const productId = productIds[Math.floor(Math.random() * productIds.length)];

  const res = http.get(`${BASE_URL}/api/products/${productId}`);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}
```

- [ ] **Step 4: Create search test**

Create `tests/load/scripts/search.js`:

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

const queries = ['laptop', 'phone', 'headphones', 'camera', 'tablet', 'watch', 'speaker'];

export const options = {
  stages: [
    { duration: '30s', target: 100 },
    { duration: '2m', target: 100 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  const query = queries[Math.floor(Math.random() * queries.length)];

  const res = http.get(`${BASE_URL}/api/search?q=${query}`);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}
```

- [ ] **Step 5: Create order-placement test**

Create `tests/load/scripts/order-placement.js`:

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
const EMAIL = __ENV.TEST_EMAIL || 'client@aemarket.com';
const PASSWORD = __ENV.TEST_PASSWORD || 'Client@12345';

let authToken = '';

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '2m', target: 50 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};

export function setup() {
  // Login and get auth token
  const loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
    email: EMAIL,
    password: PASSWORD,
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  if (loginRes.status === 200) {
    const body = JSON.parse(loginRes.body);
    authToken = body.token || body.accessToken || '';
  }

  return { token: authToken };
}

export default function (data) {
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${data.token}`,
  };

  // Get a random product to order
  const productsRes = http.get(`${BASE_URL}/api/products?page=1&pageSize=5`, { headers });
  
  if (productsRes.status === 200) {
    const products = JSON.parse(productsRes.body);
    if (products.items && products.items.length > 0) {
      const product = products.items[Math.floor(Math.random() * products.items.length)];
      
      // Place order (simplified — real test would add to cart first)
      const orderRes = http.post(`${BASE_URL}/api/orders`, JSON.stringify({
        items: [{ variantId: product.variants?.[0]?.id, quantity: 1 }],
        shippingAddress: '123 Test St',
      }), { headers });

      check(orderRes, {
        'order status is 2xx': (r) => r.status >= 200 && r.status < 300,
        'response time < 500ms': (r) => r.timings.duration < 500,
      });
    }
  }

  sleep(1);
}
```

- [ ] **Step 6: Create README**

Create `tests/load/README.md`:

```markdown
# Load Testing

## Prerequisites

- [k6](https://k6.io/docs/get-started/installation/) installed
- API running locally or in staging

## Running Tests

```bash
# Product list (200 concurrent users)
k6 run scripts/product-list.js

# Product detail (200 concurrent users)
k6 run scripts/product-detail.js

# Search (100 concurrent users)
k6 run scripts/search.js

# Order placement (50 concurrent users)
k6 run scripts/order-placement.js
```

### Custom Base URL

```bash
k6 run --env BASE_URL=http://staging.example.com:8080 scripts/product-list.js
```

## Thresholds

- **p95 < 500ms** — 95% of requests complete within 500ms
- **p99 < 1000ms** — 99% of requests complete within 1 second
- **Error rate < 1%** — Less than 1% of requests fail

## Interpreting Results

- `http_req_duration` — Request latency distribution
- `http_req_failed` — Failed request rate
- `http_reqs` — Total requests made
- `iterations` — Number of full test iterations

## Bottleneck Analysis

1. Check if p95 exceeds threshold
2. Identify slow endpoints from `http_req_duration` breakdown
3. Check server metrics (CPU, memory, database connections)
4. Review application logs for errors during test window
5. Profile slow handlers with distributed tracing (Jaeger)
```

- [ ] **Step 7: Commit**

```bash
git add tests/load/
git commit -m "feat(tests): add k6 load testing scripts with thresholds"
```

---

## Task 7: Final Verification

**Files:**
- No new files (verification only)

**Interfaces:**
- Consumes: all previous tasks
- Produces: passing test suites, valid builds

- [ ] **Step 1: Full build**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`
Expected: 0 errors

- [ ] **Step 2: Domain tests**

Run: `dotnet test .\tests\AE.Market.Domain.Tests\AE.Market.Domain.Tests.csproj --verbosity quiet`
Expected: 587+ passing

- [ ] **Step 3: Architecture tests**

Run: `dotnet test .\AE.Market.ArchitectureTests\AE.Market.ArchitectureTests.csproj --verbosity quiet`
Expected: 55/55 passing

- [ ] **Step 4: Docker compose prod validation**

Run: `docker compose -f docker-compose.prod.yml config`
Expected: Valid YAML output

- [ ] **Step 5: Update progress doc**

Update `.superpowers/sdd/progress.md` to mark Sprint 12 complete.

- [ ] **Step 6: Final commit**

```bash
git add .superpowers/sdd/progress.md
git commit -m "docs: mark Sprint 12 Production Hardening complete"
```
