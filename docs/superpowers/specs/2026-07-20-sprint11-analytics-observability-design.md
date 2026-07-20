# Sprint 11 — Analytics & Observability

**Date:** 2026-07-20
**Status:** Approved
**Depends on:** Sprint 10 (SEO & Caching) ✅

---

## Goal

Add admin dashboard statistics, distributed tracing (OpenTelemetry → Jaeger), health check endpoints, and structured analytics for product views and search queries.

## Deliverables

| Layer | What |
|-------|------|
| Domain | `Product.ViewCount` + `IncrementViewCount()` + `ProductViewedDomainEvent`; new `Analytics` aggregate with `SearchAnalytics` entity |
| Application | 2 commands (RecordProductView, RecordSearchQuery) + 3 queries (GetAdminStats, GetTopProducts, GetTopSearches) + 3 DTOs |
| Infrastructure | OpenTelemetry (OTLP → Jaeger), health checks (PG + Redis + ES), 2 event handlers |
| API | AdminController (3 endpoints), health endpoints (/health, /health/ready) |
| Tests | ~25 new tests (domain, architecture, integration) |

---

## Domain Layer

### Product entity changes

Add to `Domain/Aggregates/Catalog/Products/Product.cs`:

```csharp
public int ViewCount { get; private set; }

public void IncrementViewCount()
{
    ViewCount++;
    AddDomainEvent(new ProductViewedDomainEvent(Id));
}
```

### New Analytics aggregate

```
Domain/Aggregates/Analytics/
├── SearchAnalytics.cs
└── Events/
    └── SearchQueryLoggedDomainEvent.cs
```

**SearchAnalytics** — append-only log of every product search:

| Property | Type | Notes |
|----------|------|-------|
| `Id` | `int` | PK |
| `SearchText` | `string` | The search query |
| `Filters` | `string?` | Serialized filter state (JSON) |
| `ResultCount` | `int` | Number of results returned |
| `LatencyMs` | `long` | Search latency in milliseconds |
| `UserId` | `string?` | Nullable (anonymous users) |
| `SearchedAt` | `DateTime` | UTC timestamp |

**SearchQueryLoggedDomainEvent** — raised when SearchAnalytics is created.

### EF Configuration

- `SearchAnalyticsConfiguration` in `AE.Market.Infrastructure/Persistence/Configurations/`
- Index on `SearchedAt` for time-range queries
- Index on `SearchText` for group-by queries

### Migration

- Add `ViewCount` column to `products` table (default 0, NOT NULL)
- Create `search_analytics` table in `analytics` schema

---

## Application Layer

### Feature folder: `Application/Features/Analytics/`

```
Analytics/
├── Commands/
│   └── RecordProductView/
│       ├── RecordProductViewCommand.cs        — { ProductId: int }
│       ├── RecordProductViewHandler.cs        — loads Product, calls IncrementViewCount(), saves
│       └── RecordProductViewValidator.cs
├── Commands/
│   └── RecordSearchQuery/
│       ├── RecordSearchQueryCommand.cs        — { SearchText, Filters?, ResultCount, LatencyMs, UserId? }
│       ├── RecordSearchQueryHandler.cs        — creates SearchAnalytics, saves
│       └── RecordSearchQueryValidator.cs
├── Queries/
│   └── GetAdminStats/
│       ├── GetAdminStatsQuery.cs              — returns AdminStatsDto
│       ├── GetAdminStatsQueryHandler.cs       — DB aggregation
│       └── GetAdminStatsQueryValidator.cs
├── Queries/
│   └── GetTopProducts/
│       ├── GetTopProductsQuery.cs             — { Days: int = 30, Top: int = 10 }
│       ├── GetTopProductsQueryHandler.cs      — Product.OrderByDescending(ViewCount).Take(Top)
│       └── GetTopProductsQueryValidator.cs
├── Queries/
│   └── GetTopSearches/
│       ├── GetTopSearchesQuery.cs             — { Days: int = 30, Top: int = 10 }
│       ├── GetTopSearchesQueryHandler.cs      — SearchAnalytics.GroupBy(SearchText).OrderByDescending(Count)
│       └── GetTopSearchesQueryValidator.cs
├── DTOs/
│   ├── AdminStatsDto.cs
│   ├── TopProductDto.cs
│   └── TopSearchDto.cs
```

### DTOs

**AdminStatsDto:**
```csharp
public record AdminStatsDto(
    int TotalProducts,
    int ActiveStock,
    decimal AveragePrice,
    int TotalCategories,
    IReadOnlyList<CategoryProductCountDto> ProductsByCategory);

public record CategoryProductCountDto(string Name, int Count);
```

**TopProductDto:**
```csharp
public record TopProductDto(int ProductId, string Name, int ViewCount);
```

**TopSearchDto:**
```csharp
public record TopSearchDto(string SearchText, int Count, double AverageLatencyMs, DateTime LastSearchedAt);
```

### Design decisions

- `RecordProductViewCommand` is dispatched **fire-and-forget** from `ProductsController.GetProductById` — not via domain event
- `RecordSearchQueryCommand` is dispatched **fire-and-forget** from `SearchBehavior` after product search — supplements existing ES logging
- Admin stats queries read directly from DB (no cache — admin dashboard is low-traffic, real-time data matters)
- `GetTopSearchesQuery` uses `GROUP BY SearchText` with `COUNT(*)` and `AVG(LatencyMs)`

---

## Infrastructure Layer

### OpenTelemetry

**NuGet packages:**
- `OpenTelemetry.Extensions.Hosting`
- `OpenTelemetry.Instrumentation.AspNetCore`
- `OpenTelemetry.Instrumentation.Http`
- `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- `OpenTelemetry.Instrumentation.StackExchangeRedis`
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`

**Registration in `Infrastructure/DependencyInjection.cs`:**
```csharp
services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("AE.Market.API"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddRedisInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri("http://jaeger:4317")));
```

**Docker Compose addition (`docker-compose.override.yml`):**
```yaml
jaeger:
  image: jaegertracing/all-in-one:latest
  ports:
    - "4317:4317"    # OTLP gRPC
    - "16686:16686"  # Jaeger UI
```

### Health Checks

**NuGet packages:**
- `AspNetCore.Diagnostics.HealthChecks.NpgSql`
- `AspNetCore.Diagnostics.HealthChecks.Redis`
- `AspNetCore.Diagnostics.HealthChecks.Elasticsearch`

**Registration in `Infrastructure/DependencyInjection.cs`:**
```csharp
services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddRedis(redisConnection, name: "redis")
    .AddElasticsearch(elasticsearchUri, name: "elasticsearch");
```

**Program.cs:**
```csharp
app.MapHealthChecks("/health");                           // liveness — app is running
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")     // readiness — PG + Redis + ES
});
```

### Event Handlers

| Handler | Event | Action |
|---------|-------|--------|
| `ProductViewedEventHandler` | `ProductViewedDomainEvent` | Load Product → `IncrementViewCount()` → Save |
| `SearchQueryLoggedEventHandler` | `SearchQueryLoggedDomainEvent` | Create `SearchAnalytics` → Save |

Both registered in `Infrastructure/DependencyInjection.cs` as scoped services.

---

## API Layer

### AdminController

`AE.Market.API/Controllers/AdminController.cs`

| Endpoint | Query | Auth |
|----------|-------|------|
| `GET /api/admin/stats` | `GetAdminStatsQuery` | `[HasPermission(Permission.AccessUsers)]` |
| `GET /api/admin/stats/top-products?days=30&top=10` | `GetTopProductsQuery` | `[HasPermission(Permission.AccessUsers)]` |
| `GET /api/admin/stats/top-searches?days=30&top=10` | `GetTopSearchesQuery` | `[HasPermission(Permission.AccessUsers)]` |

### View Recording

In `ProductsController.GetProductById` (or its handler), after returning the product, fire-and-forget `RecordProductViewCommand`:

```csharp
_ = mediator.Send(new RecordProductViewCommand(product.Id), ct);
```

### Health Endpoints

- `GET /health` — liveness (no auth, returns 200 if app is running)
- `GET /health/ready` — readiness (no auth, returns 200 if PG + Redis + ES healthy, 503 otherwise)

---

## Testing

### Domain tests (~5)
- `ProductViewCountTests`: IncrementViewCount increments counter, raises domain event
- `SearchAnalyticsTests`: Factory method creates entity correctly

### Architecture tests (~3)
- Analytics feature folder follows convention (Commands/, Queries/, DTOs/)

### Integration tests (~17)
- View product → ViewCount incremented → GetAdminStats reflects count
- Search product → SearchAnalytics recorded → GetTopSearches reflects it
- GetAdminStats returns correct totals (products, stock, categories)
- GetTopProducts returns top N by ViewCount within date range
- GetTopSearches returns top N by count within date range
- Health check `/health` returns 200
- Health check `/health/ready` returns 200 when all services up
- Health check `/health/ready` returns 503 when PostgreSQL down
- Health check `/health/ready` returns 503 when Redis down
- Health check `/health/ready` returns 503 when Elasticsearch down

---

## Migration

1. Add `ViewCount` column to `products` table (int, NOT NULL, default 0)
2. Create `analytics.search_analytics` table
3. Create indexes on `search_analytics(SearchedAt)` and `search_analytics(SearchText)`

---

## Out of Scope

- Grafana dashboards (Sprint 12 or later)
- Custom metrics beyond OTel defaults
- Real-time analytics streaming
- Alerting rules
