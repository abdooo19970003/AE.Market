# Sprint 11 — Analytics & Observability Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add admin dashboard statistics, distributed tracing (OpenTelemetry → Jaeger), health check endpoints, and structured analytics for product views and search queries.

**Architecture:** Domain events flow through the outbox → MediatR → event handlers. ProductViewedDomainEvent triggers ViewCount increment on Product. SearchQueryLoggedDomainEvent persists SearchAnalytics to DB. AdminController queries aggregated data. OpenTelemetry traces HTTP/EF Core/Redis and exports to Jaeger via OTLP. Health checks verify PostgreSQL, Redis, and Elasticsearch connectivity.

**Tech Stack:** ASP.NET Core 10, EF Core 10, PostgreSQL 17, Elasticsearch 8, MediatR, FluentValidation, OpenTelemetry + OTLP exporter, AspNetCore.Diagnostics.HealthChecks, xUnit + FluentAssertions

## Global Constraints

- File-scoped namespaces (`namespace X.Y;`)
- Primary constructors for DI
- `Async` suffix on async methods, `CancellationToken` as last param
- Commands implement `ICommand<T>` or `ICommand`, queries implement `IBaseQuery<T>`
- Handlers are `internal sealed class`, validators are `public sealed class`
- Domain events use `AddDomainEvent()` in entity methods, dispatched via outbox
- All read queries use `AsNoTracking()` via repository
- Controllers are `sealed class` with `[Route("api/[controller]")]`
- Use `Result<T>` / `Result` pattern — never throw for control flow

---

## Task 1: Domain Layer — Product.ViewCount + SearchAnalytics

**Files:**
- Modify: `Domain/Aggregates/Catalog/Products/Product.cs`
- Create: `Domain/Aggregates/Analytics/SearchAnalytics.cs`
- Create: `Domain/Aggregates/Analytics/Events/ProductViewedDomainEvent.cs`
- Create: `Domain/Aggregates/Analytics/Events/SearchQueryLoggedDomainEvent.cs`
- Test: `tests/AE.Market.Domain.Tests/Aggregates/Analytics/ProductViewCountTests.cs`
- Test: `tests/AE.Market.Domain.Tests/Aggregates/Analytics/SearchAnalyticsTests.cs`

**Interfaces:**
- Consumes: `BaseEntity` (base class), `IDomainEvent` (marker interface)
- Produces: `Product.ViewCount`, `Product.IncrementViewCount()`, `ProductViewedDomainEvent`, `SearchAnalytics` entity, `SearchQueryLoggedDomainEvent`

- [ ] **Step 1: Add ViewCount to Product entity**

Open `Domain/Aggregates/Catalog/Products/Product.cs`. Add after the `OgImage` property (around line 40):

```csharp
public int ViewCount { get; private set; }
```

Add method after `SetOgImage` (around line 338):

```csharp
public void IncrementViewCount()
{
    ViewCount++;
    AddDomainEvent(new ProductViewedDomainEvent(Id));
}
```

Add using at top:

```csharp
using AE.Market.Domain.Aggregates.Analytics.Events;
```

- [ ] **Step 2: Create ProductViewedDomainEvent**

Create `Domain/Aggregates/Analytics/Events/ProductViewedDomainEvent.cs`:

```csharp
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Analytics.Events;

public sealed record ProductViewedDomainEvent(Guid ProductId) : IDomainEvent;
```

- [ ] **Step 3: Create SearchAnalytics entity**

Create `Domain/Aggregates/Analytics/SearchAnalytics.cs`:

```csharp
using AE.Market.Domain.Aggregates.Analytics.Events;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Analytics;

public sealed class SearchAnalytics : BaseEntity, IAggregateRoot
{
    public string SearchText { get; private set; } = default!;
    public string? Filters { get; private set; }
    public int ResultCount { get; private set; }
    public long LatencyMs { get; private set; }
    public string? UserId { get; private set; }
    public DateTime SearchedAt { get; private set; }

    private SearchAnalytics() { }

    public static SearchAnalytics Create(
        string searchText,
        string? filters,
        int resultCount,
        long latencyMs,
        string? userId)
    {
        var entity = new SearchAnalytics
        {
            Id = Guid.NewGuid(),
            SearchText = searchText,
            Filters = filters,
            ResultCount = resultCount,
            LatencyMs = latencyMs,
            UserId = userId,
            SearchedAt = DateTime.UtcNow
        };

        entity.AddDomainEvent(new SearchQueryLoggedDomainEvent(entity.Id));
        return entity;
    }
}
```

- [ ] **Step 4: Create SearchQueryLoggedDomainEvent**

Create `Domain/Aggregates/Analytics/Events/SearchQueryLoggedDomainEvent.cs`:

```csharp
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Analytics.Events;

public sealed record SearchQueryLoggedDomainEvent(Guid SearchAnalyticsId) : IDomainEvent;
```

- [ ] **Step 5: Write domain tests for Product ViewCount**

Create `tests/AE.Market.Domain.Tests/Aggregates/Analytics/ProductViewCountTests.cs`:

```csharp
using AE.Market.Domain.Aggregates.Analytics.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Analytics;

public sealed class ProductViewCountTests
{
    private static Product CreateTestProduct()
    {
        return Product.Create(
            Guid.NewGuid(),
            "Test Product",
            Catalog.ValueObjects.Slug.Create("test-product"),
            Catalog.ValueObjects.Sku.Create("TP-001"),
            Guid.NewGuid(),
            ProductType.Simple,
            "Test details");
    }

    [Fact]
    public void IncrementViewCount_IncreasesCountByOne()
    {
        var product = CreateTestProduct();

        product.IncrementViewCount();

        product.ViewCount.Should().Be(1);
    }

    [Fact]
    public void IncrementViewCount_CalledTwice_IncreasesToTwo()
    {
        var product = CreateTestProduct();

        product.IncrementViewCount();
        product.IncrementViewCount();

        product.ViewCount.Should().Be(2);
    }

    [Fact]
    public void IncrementViewCount_RaisesProductViewedDomainEvent()
    {
        var product = CreateTestProduct();
        product.ClearDomainEvents();

        product.IncrementViewCount();

        product.DomainEvents.Should().Contain(e => e is ProductViewedDomainEvent);
        var evt = product.DomainEvents.OfType<ProductViewedDomainEvent>().Single();
        evt.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public void ViewCount_DefaultsToZero()
    {
        var product = CreateTestProduct();

        product.ViewCount.Should().Be(0);
    }
}
```

- [ ] **Step 6: Write domain tests for SearchAnalytics**

Create `tests/AE.Market.Domain.Tests/Aggregates/Analytics/SearchAnalyticsTests.cs`:

```csharp
using AE.Market.Domain.Aggregates.Analytics;
using AE.Market.Domain.Aggregates.Analytics.Events;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Analytics;

public sealed class SearchAnalyticsTests
{
    [Fact]
    public void Create_WithValidData_ReturnsEntity()
    {
        var entity = SearchAnalytics.Create(
            "laptop",
            """{"categoryId":"abc"}""",
            15,
            42,
            "user-1");

        entity.SearchText.Should().Be("laptop");
        entity.Filters.Should().Be("""{"categoryId":"abc"}""");
        entity.ResultCount.Should().Be(15);
        entity.LatencyMs.Should().Be(42);
        entity.UserId.Should().Be("user-1");
        entity.SearchedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithNullFilters_SetsNull()
    {
        var entity = SearchAnalytics.Create("phone", null, 5, 10, null);

        entity.Filters.Should().BeNull();
        entity.UserId.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesSearchQueryLoggedDomainEvent()
    {
        var entity = SearchAnalytics.Create("test", null, 0, 0, null);

        entity.DomainEvents.Should().Contain(e => e is SearchQueryLoggedDomainEvent);
        var evt = entity.DomainEvents.OfType<SearchQueryLoggedDomainEvent>().Single();
        evt.SearchAnalyticsId.Should().Be(entity.Id);
    }
}
```

- [ ] **Step 7: Run domain tests to verify they pass**

Run: `dotnet test .\tests\AE.Market.Domain.Tests\AE.Market.Domain.Tests.csproj --filter "FullyQualifiedName~Analytics" --verbosity quiet`

Expected: All 7 tests pass.

- [ ] **Step 8: Run full build to verify no compilation errors**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`

Expected: Build succeeds with 0 errors.

- [ ] **Step 9: Commit**

```bash
git add Domain/Aggregates/Catalog/Products/Product.cs Domain/Aggregates/Analytics/ tests/AE.Market.Domain.Tests/Aggregates/Analytics/
git commit -m "feat(domain): add Product.ViewCount and SearchAnalytics entity with domain events"
```

---

## Task 2: EF Core — Configuration + Migration

**Files:**
- Create: `AE.Market.Infrastructure/Persistence/Configurations/Analytics/SearchAnalyticsConfiguration.cs`
- Modify: `AE.Market.Infrastructure/Persistence/AppDbContext.cs`
- Create: `AE.Market.Infrastructure/Persistence/Migrations/` (auto-generated)

**Interfaces:**
- Consumes: `SearchAnalytics` entity (Task 1)
- Produces: `search_analytics` table, `ViewCount` column on products, EF migrations

- [ ] **Step 1: Create SearchAnalyticsConfiguration**

Create `AE.Market.Infrastructure/Persistence/Configurations/Analytics/SearchAnalyticsConfiguration.cs`:

```csharp
using AE.Market.Domain.Aggregates.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Analytics;

internal sealed class SearchAnalyticsConfiguration : IEntityTypeConfiguration<SearchAnalytics>
{
    public void Configure(EntityTypeBuilder<SearchAnalytics> builder)
    {
        builder.ToTable("search_analytics", "analytics");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SearchText).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Filters).HasMaxLength(2000);
        builder.Property(x => x.ResultCount).HasDefaultValue(0);
        builder.Property(x => x.LatencyMs).HasDefaultValue(0);
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.SearchedAt).IsRequired();

        builder.HasIndex(x => x.SearchedAt);
        builder.HasIndex(x => x.SearchText);
    }
}
```

- [ ] **Step 2: Add DbSet to AppDbContext**

Open `AE.Market.Infrastructure/Persistence/AppDbContext.cs`. Add using at top:

```csharp
using AE.Market.Domain.Aggregates.Analytics;
```

Add DbSet after the Inventory DbSet (around line 65):

```csharp
// Analytics Schema
public DbSet<SearchAnalytics> SearchAnalytics { get; set; }
```

- [ ] **Step 3: Add ViewCount to ProductConfiguration**

Find the Product configuration file. Check `AE.Market.Infrastructure/Persistence/Configurations/Catalog/` for a Product configuration. If it exists, add:

```csharp
builder.Property(x => x.ViewCount).HasDefaultValue(0);
```

If no Product configuration exists, the `ViewCount` column will be created via convention (int, NOT NULL, default 0 via EF convention).

- [ ] **Step 4: Create EF Core migration**

Run: `dotnet ef migrations add Sprint11_Analytics --project AE.Market.Infrastructure --startup-project AE.Market.API --output-dir Persistence/Migrations`

Expected: Migration created with `ViewCount` column on products and `search_analytics` table.

- [ ] **Step 5: Verify migration compiles**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`

Expected: Build succeeds.

- [ ] **Step 6: Commit**

```bash
git add AE.Market.Infrastructure/Persistence/ AE.Market.Infrastructure/Persistence/Migrations/
git commit -m "feat(infra): add SearchAnalytics EF config and ViewCount migration"
```

---

## Task 3: Application — Commands (RecordProductView + RecordSearchQuery)

**Files:**
- Create: `Application/Features/Analytics/Commands/RecordProductView/RecordProductViewCommand.cs`
- Create: `Application/Features/Analytics/Commands/RecordProductView/RecordProductViewHandler.cs`
- Create: `Application/Features/Analytics/Commands/RecordSearchQuery/RecordSearchQueryCommand.cs`
- Create: `Application/Features/Analytics/Commands/RecordSearchQuery/RecordSearchQueryHandler.cs`

**Interfaces:**
- Consumes: `IRepository<Product>` (Task 1), `IRepository<SearchAnalytics>` (Task 1), `ICurrentUser` (existing)
- Produces: `RecordProductViewCommand`, `RecordSearchQueryCommand`

- [ ] **Step 1: Create RecordProductViewCommand**

Create `Application/Features/Analytics/Commands/RecordProductView/RecordProductViewCommand.cs`:

```csharp
using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Analytics.Commands.RecordProductView;

public sealed record RecordProductViewCommand(Guid ProductId) : ICommand;
```

- [ ] **Step 2: Create RecordProductViewHandler**

Create `Application/Features/Analytics/Commands/RecordProductView/RecordProductViewHandler.cs`:

```csharp
using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Commands.RecordProductView;

internal sealed class RecordProductViewHandler(
    IRepository<Product> repo
) : IRequestHandler<RecordProductViewCommand, Result>
{
    public async Task<Result> Handle(RecordProductViewCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Fail(new Error("Application.NotFound", "Product not found"));

        product.IncrementViewCount();
        await repo.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

- [ ] **Step 3: Create RecordSearchQueryCommand**

Create `Application/Features/Analytics/Commands/RecordSearchQuery/RecordSearchQueryCommand.cs`:

```csharp
using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Analytics.Commands.RecordSearchQuery;

public sealed record RecordSearchQueryCommand(
    string SearchText,
    string? Filters,
    int ResultCount,
    long LatencyMs,
    string? UserId
) : ICommand;
```

- [ ] **Step 4: Create RecordSearchQueryHandler**

Create `Application/Features/Analytics/Commands/RecordSearchQuery/RecordSearchQueryHandler.cs`:

```csharp
using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Analytics;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Commands.RecordSearchQuery;

internal sealed class RecordSearchQueryHandler(
    IRepository<SearchAnalytics> repo
) : IRequestHandler<RecordSearchQueryCommand, Result>
{
    public async Task<Result> Handle(RecordSearchQueryCommand request, CancellationToken cancellationToken)
    {
        var entity = SearchAnalytics.Create(
            request.SearchText,
            request.Filters,
            request.ResultCount,
            request.LatencyMs,
            request.UserId);

        await repo.AddAsync(entity, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

- [ ] **Step 5: Build to verify compilation**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`

Expected: Build succeeds.

- [ ] **Step 6: Commit**

```bash
git add Application/Features/Analytics/Commands/
git commit -m "feat(app): add RecordProductView and RecordSearchQuery commands"
```

---

## Task 4: Application — Queries + DTOs (AdminStats, TopProducts, TopSearches)

**Files:**
- Create: `Application/Features/Analytics/DTOs/AdminStatsDto.cs`
- Create: `Application/Features/Analytics/DTOs/TopProductDto.cs`
- Create: `Application/Features/Analytics/DTOs/TopSearchDto.cs`
- Create: `Application/Features/Analytics/Queries/GetAdminStats/GetAdminStatsQuery.cs`
- Create: `Application/Features/Analytics/Queries/GetAdminStats/GetAdminStatsQueryHandler.cs`
- Create: `Application/Features/Analytics/Queries/GetTopProducts/GetTopProductsQuery.cs`
- Create: `Application/Features/Analytics/Queries/GetTopProducts/GetTopProductsQueryHandler.cs`
- Create: `Application/Features/Analytics/Queries/GetTopSearches/GetTopSearchesQuery.cs`
- Create: `Application/Features/Analytics/Queries/GetTopSearches/GetTopSearchesQueryHandler.cs`

**Interfaces:**
- Consumes: `IReadRepository<Product>` (existing), `IReadRepository<SearchAnalytics>` (Task 1)
- Produces: `AdminStatsDto`, `TopProductDto`, `TopSearchDto`, 3 query records + handlers

- [ ] **Step 1: Create DTOs**

Create `Application/Features/Analytics/DTOs/AdminStatsDto.cs`:

```csharp
namespace AE.Market.Application.Features.Analytics.DTOs;

public sealed record AdminStatsDto(
    int TotalProducts,
    int ActiveStock,
    decimal AveragePrice,
    int TotalCategories,
    IReadOnlyList<CategoryProductCountDto> ProductsByCategory);

public sealed record CategoryProductCountDto(string Name, int Count);
```

Create `Application/Features/Analytics/DTOs/TopProductDto.cs`:

```csharp
namespace AE.Market.Application.Features.Analytics.DTOs;

public sealed record TopProductDto(Guid ProductId, string Name, int ViewCount);
```

Create `Application/Features/Analytics/DTOs/TopSearchDto.cs`:

```csharp
namespace AE.Market.Application.Features.Analytics.DTOs;

public sealed record TopSearchDto(string SearchText, int Count, double AverageLatencyMs, DateTime LastSearchedAt);
```

- [ ] **Step 2: Create GetAdminStatsQuery**

Create `Application/Features/Analytics/Queries/GetAdminStats/GetAdminStatsQuery.cs`:

```csharp
using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Analytics.DTOs;

namespace AE.Market.Application.Features.Analytics.Queries.GetAdminStats;

public sealed record GetAdminStatsQuery : IBaseQuery<AdminStatsDto>;
```

- [ ] **Step 3: Create GetAdminStatsQueryHandler**

Create `Application/Features/Analytics/Queries/GetAdminStats/GetAdminStatsQueryHandler.cs`:

```csharp
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Analytics.DTOs;
using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Application.Features.Analytics.Queries.GetAdminStats;

internal sealed class GetAdminStatsQueryHandler(
    IReadRepository<Product> productRepo,
    IReadRepository<Category> categoryRepo,
    AppDbContext dbContext
) : IRequestHandler<GetAdminStatsQuery, Result<AdminStatsDto>>
{
    public async Task<Result<AdminStatsDto>> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var products = dbContext.Products.Where(p => !p.IsDeleted);

        var totalProducts = await products.CountAsync(cancellationToken);

        var activeStock = await products
            .Where(p => p.Status == ProductStatus.Active)
            .SumAsync(p => p.StockQuantity, cancellationToken);

        var averagePrice = await products
            .Where(p => p.Status == ProductStatus.Active)
            .AverageAsync(p => (decimal?)p.ListPrice, cancellationToken) ?? 0m;

        var totalCategories = await dbContext.Categories
            .Where(c => !c.IsDeleted)
            .CountAsync(cancellationToken);

        var productsByCategory = await dbContext.Categories
            .Where(c => !c.IsDeleted)
            .Select(c => new CategoryProductCountDto(
                c.Name,
                dbContext.Products.Count(p => p.CategoryId == c.Id && !p.IsDeleted)))
            .ToListAsync(cancellationToken);

        var dto = new AdminStatsDto(
            totalProducts,
            activeStock,
            averagePrice,
            totalCategories,
            productsByCategory);

        return Result<AdminStatsDto>.Success(dto);
    }
}
```

- [ ] **Step 4: Create GetTopProductsQuery**

Create `Application/Features/Analytics/Queries/GetTopProducts/GetTopProductsQuery.cs`:

```csharp
using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Analytics.DTOs;

namespace AE.Market.Application.Features.Analytics.Queries.GetTopProducts;

public sealed record GetTopProductsQuery(
    int Days = 30,
    int Top = 10
) : IBaseQuery<IReadOnlyList<TopProductDto>>;
```

- [ ] **Step 5: Create GetTopProductsQueryHandler**

Create `Application/Features/Analytics/Queries/GetTopProducts/GetTopProductsQueryHandler.cs`:

```csharp
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Analytics.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Application.Features.Analytics.Queries.GetTopProducts;

internal sealed class GetTopProductsQueryHandler(
    AppDbContext dbContext
) : IRequestHandler<GetTopProductsQuery, Result<IReadOnlyList<TopProductDto>>>
{
    public async Task<Result<IReadOnlyList<TopProductDto>>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-request.Days);

        var topProducts = await dbContext.Products
            .Where(p => !p.IsDeleted && p.CreatedAt >= cutoff)
            .OrderByDescending(p => p.ViewCount)
            .Take(request.Top)
            .Select(p => new TopProductDto(p.Id, p.Name, p.ViewCount))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TopProductDto>>.Success(topProducts);
    }
}
```

- [ ] **Step 6: Create GetTopSearchesQuery**

Create `Application/Features/Analytics/Queries/GetTopSearches/GetTopSearchesQuery.cs`:

```csharp
using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Analytics.DTOs;

namespace AE.Market.Application.Features.Analytics.Queries.GetTopSearches;

public sealed record GetTopSearchesQuery(
    int Days = 30,
    int Top = 10
) : IBaseQuery<IReadOnlyList<TopSearchDto>>;
```

- [ ] **Step 7: Create GetTopSearchesQueryHandler**

Create `Application/Features/Analytics/Queries/GetTopSearches/GetTopSearchesQueryHandler.cs`:

```csharp
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Analytics.DTOs;
using AE.Market.Domain.Aggregates.Analytics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Application.Features.Analytics.Queries.GetTopSearches;

internal sealed class GetTopSearchesQueryHandler(
    AppDbContext dbContext
) : IRequestHandler<GetTopSearchesQuery, Result<IReadOnlyList<TopSearchDto>>>
{
    public async Task<Result<IReadOnlyList<TopSearchDto>>> Handle(GetTopSearchesQuery request, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-request.Days);

        var topSearches = await dbContext.SearchAnalytics
            .Where(s => s.SearchedAt >= cutoff)
            .GroupBy(s => s.SearchText)
            .Select(g => new TopSearchDto(
                g.Key,
                g.Count(),
                g.Average(x => (double)x.LatencyMs),
                g.Max(x => x.SearchedAt)))
            .OrderByDescending(x => x.Count)
            .Take(request.Top)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TopSearchDto>>.Success(topSearches);
    }
}
```

- [ ] **Step 8: Build to verify compilation**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`

Expected: Build succeeds.

- [ ] **Step 9: Commit**

```bash
git add Application/Features/Analytics/DTOs/ Application/Features/Analytics/Queries/
git commit -m "feat(app): add admin stats queries and DTOs"
```

---

## Task 5: API — AdminController + View Recording + Health Endpoints

**Files:**
- Create: `AE.Market.API/Controllers/AdminController.cs`
- Modify: `AE.Market.API/Controllers/ProductsController.cs` (add view recording)
- Modify: `AE.Market.API/Program.cs` (add health endpoints)

**Interfaces:**
- Consumes: `IMediator` (existing), `GetAdminStatsQuery`, `GetTopProductsQuery`, `GetTopSearchesQuery` (Task 4), `RecordProductViewCommand` (Task 3)
- Produces: AdminController, updated ProductsController, health endpoints

- [ ] **Step 1: Create AdminController**

Create `AE.Market.API/Controllers/AdminController.cs`:

```csharp
using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Analytics.Queries.GetAdminStats;
using AE.Market.Application.Features.Analytics.Queries.GetTopProducts;
using AE.Market.Application.Features.Analytics.Queries.GetTopSearches;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class AdminController(IMediator mediator) : ControllerBase
{
    [HttpGet("stats")]
    [HasPermission(Permission.AccessUsers)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminStatsQuery(), ct);
        return result.ToActionResult();
    }

    [HttpGet("stats/top-products")]
    [HasPermission(Permission.AccessUsers)]
    public async Task<IActionResult> GetTopProducts([FromQuery] int days = 30, [FromQuery] int top = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTopProductsQuery(days, top), ct);
        return result.ToActionResult();
    }

    [HttpGet("stats/top-searches")]
    [HasPermission(Permission.AccessUsers)]
    public async Task<IActionResult> GetTopSearches([FromQuery] int days = 30, [FromQuery] int top = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTopSearchesQuery(days, top), ct);
        return result.ToActionResult();
    }
}
```

- [ ] **Step 2: Add view recording to ProductsController**

Open `AE.Market.API/Controllers/ProductsController.cs`. Add using at top:

```csharp
using AE.Market.Application.Features.Analytics.Commands.RecordProductView;
```

Modify `GetProductById` method (around line 47-51):

```csharp
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetProductById(Guid id, CancellationToken ct)
{
    var result = await mediator.Send(new GetProductByIdQuery(id), ct);
    _ = mediator.Send(new RecordProductViewCommand(id), ct);
    return result.ToNotFoundActionResult();
}
```

- [ ] **Step 3: Add health endpoints to Program.cs**

Open `AE.Market.API/Program.cs`. Add using at top:

```csharp
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
```

Add health endpoints before `app.Run()` (after `app.MapControllers()`):

```csharp
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

- [ ] **Step 4: Build to verify compilation**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`

Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add AE.Market.API/Controllers/AdminController.cs AE.Market.API/Controllers/ProductsController.cs AE.Market.API/Program.cs
git commit -m "feat(api): add AdminController, view recording, and health endpoints"
```

---

## Task 6: OpenTelemetry + Health Checks (Infrastructure)

**Files:**
- Modify: `AE.Market.Infrastructure/AE.Market.Infrastructure.csproj` (add NuGet packages)
- Modify: `AE.Market.Infrastructure/DependencyInjection.cs` (add OTel + health checks)
- Modify: `docker-compose.override.yml` (add Jaeger)

**Interfaces:**
- Consumes: `AppDbContext` (existing), `IConnectionMultiplexer` (existing), Elasticsearch URI (existing config)
- Produces: OpenTelemetry tracing, health check endpoints, Jaeger service

- [ ] **Step 1: Add NuGet packages to Infrastructure project**

Run: `dotnet add .\AE.Market.Infrastructure\AE.Market.Infrastructure.csproj package OpenTelemetry.Extensions.Hosting --version 1.11.2`

Run: `dotnet add .\AE.Market.Infrastructure\AE.Market.Infrastructure.csproj package OpenTelemetry.Instrumentation.AspNetCore --version 1.11.2`

Run: `dotnet add .\AE.Market.Infrastructure\AE.Market.Infrastructure.csproj package OpenTelemetry.Instrumentation.Http --version 1.11.2`

Run: `dotnet add .\AE.Market.Infrastructure\AE.Market.Infrastructure.csproj package OpenTelemetry.Instrumentation.EntityFrameworkCore --version 1.11.2-beta.1`

Run: `dotnet add .\AE.Market.Infrastructure\AE.Market.Infrastructure.csproj package OpenTelemetry.Instrumentation.StackExchangeRedis --version 1.11.2`

Run: `dotnet add .\AE.Market.Infrastructure\AE.Market.Infrastructure.csproj package OpenTelemetry.Exporter.OpenTelemetryProtocol --version 1.11.2`

Run: `dotnet add .\AE.Market.Infrastructure\AE.Market.Infrastructure.csproj package AspNetCore.Diagnostics.HealthChecks.NpgSql --version 9.0.0`

Run: `dotnet add .\AE.Market.Infrastructure\AE.Market.Infrastructure.csproj package AspNetCore.Diagnostics.HealthChecks.Redis --version 9.0.0`

Run: `dotnet add .\AE.Market.Infrastructure\AE.Market.Infrastructure.csproj package AspNetCore.Diagnostics.HealthChecks.Elasticsearch --version 9.0.0`

- [ ] **Step 2: Add OpenTelemetry to Infrastructure DI**

Open `AE.Market.Infrastructure/DependencyInjection.cs`. Add usings at top:

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
```

Add `AddObservability()` method call in `AddInfrastructureServices` (after `.AddSearch(configuration)`):

```csharp
services.AddObservability(configuration);
```

Add the private method at the bottom of the class:

```csharp
private static IServiceCollection AddObservability(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var otelEndpoint = configuration["OpenTelemetry:Endpoint"] ?? "http://jaeger:4317";

    services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("AE.Market.API"))
        .WithTracing(t => t
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint)));

    services.AddHealthChecks()
        .AddNpgSql(
            configuration.GetConnectionString("Database")!,
            name: "postgresql",
            tags: ["ready"])
        .AddRedis(
            configuration.GetConnectionString("redis")!,
            name: "redis",
            tags: ["ready"])
        .AddElasticsearch(
            configuration["Elasticsearch:Uri"] ?? "http://localhost:9200",
            name: "elasticsearch",
            tags: ["ready"]);

    return services;
}
```

- [ ] **Step 3: Add Jaeger to docker-compose.override.yml**

Open `docker-compose.override.yml`. Add at the end (inside the `services` block):

```yaml
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "4317:4317"
      - "16686:16686"
```

- [ ] **Step 4: Build to verify compilation**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`

Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add AE.Market.Infrastructure/AE.Market.Infrastructure.csproj AE.Market.Infrastructure/DependencyInjection.cs docker-compose.override.yml
git commit -m "feat(infra): add OpenTelemetry, health checks, and Jaeger service"
```

---

## Task 7: Event Handlers (ProductViewed + SearchQueryLogged)

**Files:**
- Create: `Application/Features/Analytics/Events/ProductViewedEventHandler.cs`
- Create: `Application/Features/Analytics/Events/SearchQueryLoggedEventHandler.cs`
- Modify: `AE.Market.Infrastructure/DependencyInjection.cs` (register handlers)

**Interfaces:**
- Consumes: `DomainEventNotification<ProductViewedDomainEvent>` (Task 1), `DomainEventNotification<SearchQueryLoggedDomainEvent>` (Task 1), `IRepository<Product>` (existing), `IRepository<SearchAnalytics>` (Task 1)
- Produces: Event handlers registered in DI

- [ ] **Step 1: Create ProductViewedEventHandler**

Create `Application/Features/Analytics/Events/ProductViewedEventHandler.cs`:

```csharp
using AE.Market.Application.Common.Behaviors;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Events;

// No-op handler: ViewCount is already incremented by RecordProductViewHandler.
// This handler exists so the domain event is acknowledged by the outbox pipeline
// and doesn't cause "unhandled notification" warnings.
internal sealed class ProductViewedEventHandler
    : INotificationHandler<DomainEventNotification<Domain.Aggregates.Analytics.Events.ProductViewedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<Domain.Aggregates.Analytics.Events.ProductViewedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

**Why no-op?** The flow is: Controller → RecordProductViewCommand handler → `IncrementViewCount()` → SaveChanges → outbox writes domain event → OutboxProcessor publishes → this handler runs. If this handler also called `IncrementViewCount()`, the count would double. The command handler already did the work.

- [ ] **Step 2: Create SearchQueryLoggedEventHandler**

Create `Application/Features/Analytics/Events/SearchQueryLoggedEventHandler.cs`:

```csharp
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Analytics;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Events;

internal sealed class SearchQueryLoggedEventHandler(
    IRepository<SearchAnalytics> repo
) : INotificationHandler<DomainEventNotification<Domain.Aggregates.Analytics.Events.SearchQueryLoggedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<Domain.Aggregates.Analytics.Events.SearchQueryLoggedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        // SearchAnalytics is already persisted by RecordSearchQueryHandler
        // This handler is for any future side effects (e.g., notifications, alerts)
        // Currently a no-op — placeholder for future analytics processing
        await Task.CompletedTask;
    }
}
```

- [ ] **Step 3: Register event handlers in Infrastructure DI**

Open `AE.Market.Infrastructure/DependencyInjection.cs`. Add usings:

```csharp
using AE.Market.Application.Features.Analytics.Events;
```

Add in `AddInfrastructureServices` method (after the existing service registrations):

```csharp
// Analytics event handlers
services.AddScoped<INotificationHandler<DomainEventNotification<Domain.Aggregates.Analytics.Events.ProductViewedDomainEvent>>, ProductViewedEventHandler>();
services.AddScoped<INotificationHandler<DomainEventNotification<Domain.Aggregates.Analytics.Events.SearchQueryLoggedDomainEvent>>, SearchQueryLoggedEventHandler>();
```

Add using:

```csharp
using AE.Market.Application.Common.Behaviors;
using MediatR;
```

- [ ] **Step 4: Build to verify compilation**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`

Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add Application/Features/Analytics/Events/ AE.Market.Infrastructure/DependencyInjection.cs
git commit -m "feat(app): add analytics event handlers and register in DI"
```

---

## Task 8: Architecture Tests

**Files:**
- Modify: `AE.Market.ArchitectureTests/Application/FeatureStructureTests.cs` (verify Analytics passes)

**Interfaces:**
- Consumes: Application assembly (existing)
- Produces: Architecture tests passing for Analytics feature

- [ ] **Step 1: Run architecture tests to verify Analytics passes**

Run: `dotnet test .\AE.Market.ArchitectureTests\AE.Market.ArchitectureTests.csproj --verbosity quiet`

Expected: All 55 tests pass (Analytics feature should now have Commands/, Queries/, DTOs/ folders).

- [ ] **Step 2: No commit needed — this is verification only**

---

## Task 9: Integration Tests

**Files:**
- Create: `tests/AE.Market.Integration.Tests/Analytics/AdminStatsTests.cs`
- Create: `tests/AE.Market.Integration.Tests/Analytics/HealthCheckTests.cs`

**Interfaces:**
- Consumes: `IntegrationTestWebAppFactory` (existing), Admin endpoints, health endpoints
- Produces: Integration test coverage

- [ ] **Step 1: Create AdminStatsTests**

Create `tests/AE.Market.Integration.Tests/Analytics/AdminStatsTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Integration.Tests;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AE.Market.Integration.Tests.Analytics;

public class AdminStatsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AdminStatsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStats_ReturnsSuccessAndStats()
    {
        var response = await _client.GetAsync("/api/admin/stats");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("totalProducts");
        content.Should().Contain("activeStock");
        content.Should().Contain("averagePrice");
    }

    [Fact]
    public async Task GetTopProducts_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/admin/stats/top-products?days=30&top=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTopSearches_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/admin/stats/top-searches?days=30&top=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStats_WithoutAuth_ReturnsUnauthorized()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        var response = await client.GetAsync("/api/admin/stats");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

- [ ] **Step 2: Create HealthCheckTests**

Create `tests/AE.Market.Integration.Tests/Analytics/HealthCheckTests.cs`:

```csharp
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AE.Market.Integration.Tests.Analytics;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReady_ReturnsSuccessOrServiceUnavailable()
    {
        var response = await _client.GetAsync("/health/ready");
        // In integration test env, services may not be running
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Health_WithoutAuth_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

- [ ] **Step 3: Run integration tests**

Run: `dotnet test .\tests\AE.Market.Integration.Tests\AE.Market.Integration.Tests.csproj --filter "FullyQualifiedName~Analytics" --verbosity quiet`

Expected: Tests pass (some may be skipped if infrastructure isn't running).

- [ ] **Step 4: Commit**

```bash
git add tests/AE.Market.Integration.Tests/Analytics/
git commit -m "test(integration): add admin stats and health check integration tests"
```

---

## Task 10: Final Verification + Documentation

**Files:**
- No file changes (verification only)
- Update: `.superpowers/sdd/progress.md` (mark Sprint 11 complete)

- [ ] **Step 1: Run full build**

Run: `dotnet build .\AE.Market.slnx --verbosity quiet`

Expected: Build succeeds with 0 errors.

- [ ] **Step 2: Run all domain tests**

Run: `dotnet test .\tests\AE.Market.Domain.Tests\AE.Market.Domain.Tests.csproj --verbosity quiet`

Expected: All tests pass (580+ from Sprint 10 + 7 new = ~587).

- [ ] **Step 3: Run all architecture tests**

Run: `dotnet test .\AE.Market.ArchitectureTests\AE.Market.ArchitectureTests.csproj --verbosity quiet`

Expected: All 55 tests pass.

- [ ] **Step 4: Update progress doc**

Update `.superpowers/sdd/progress.md` to mark Sprint 11 complete.

- [ ] **Step 5: Final commit**

```bash
git add .superpowers/sdd/progress.md
git commit -m "docs: mark Sprint 11 Analytics & Observability complete"
```
