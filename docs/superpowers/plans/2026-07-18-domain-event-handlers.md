# Domain Event Handlers Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement 11 missing domain event handlers for cache invalidation, cross-aggregate sync, and stock release.

**Architecture:** Each handler is an `internal sealed class` implementing `INotificationHandler<DomainEventNotification<TEvent>>`. Handlers inject `ICacheService` for cache flush and/or `IRepository<T>` for cross-aggregate sync. No domain changes — all events are already defined and raised.

**Tech Stack:** .NET 10, MediatR, xUnit, FluentAssertions

## Global Constraints

- File-scoped namespaces, sealed classes, primary constructors for DI
- Handlers are `internal sealed class` in `Application/Features/{Aggregate}/Events/`
- Cache invalidation via `ICacheService.RemoveAsync()`
- Cross-aggregate sync via `IRepository<T>.GetBySpecWithTrackingAsync()` + entity method calls
- No domain logic in handlers — only cache flush and cross-aggregate sync
- Existing handler patterns: see `VariantPriceChangedEventHandler`, `InventoryAdjustedHandler`, `OrderPlacedHandler`

## File Structure

```
Application/Features/Pricing/Events/PriceCreatedHandler.cs
Application/Features/Pricing/Events/PriceUpdatedHandler.cs
Application/Features/Pricing/Events/PriceDeletedHandler.cs
Application/Features/Orders/Events/OrderCancelledHandler.cs
Application/Features/Inventory/Events/InventoryCreatedHandler.cs
Application/Features/Inventory/Events/OutOfStockHandler.cs
Application/Features/Inventory/Events/StockReservedHandler.cs
Application/Features/Inventory/Events/StockReleasedHandler.cs
Application/Features/Inventory/Events/LowStockHandler.cs
Application/Features/Catalog/Events/VariantActivatedHandler.cs
Application/Features/Catalog/Events/VariantDeactivatedHandler.cs
```

---

### Task 1: Price Event Handlers (Cache Invalidation)

**Files:**
- Create: `Application/Features/Pricing/Events/PriceCreatedHandler.cs`
- Create: `Application/Features/Pricing/Events/PriceUpdatedHandler.cs`
- Create: `Application/Features/Pricing/Events/PriceDeletedHandler.cs`

**Interfaces:**
- Consumes: `ICacheService` (from `Application/Services/ICacheService.cs`), `CacheKeys.ActivePrice(Guid, Guid?)` (from `Application/Features/Pricing/CacheKeys.cs`)
- Produces: Three handlers that flush price cache on create/update/delete

- [ ] **Step 1: Write PriceCreatedHandler**

```csharp
// Application/Features/Pricing/Events/PriceCreatedHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Prices.Events;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Events;

internal sealed class PriceCreatedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<PriceCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PriceCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ActivePrice(evt.VariantId), cancellationToken);
    }
}
```

- [ ] **Step 2: Write PriceUpdatedHandler**

```csharp
// Application/Features/Pricing/Events/PriceUpdatedHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Prices.Events;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Events;

internal sealed class PriceUpdatedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<PriceUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PriceUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ActivePrice(evt.VariantId), cancellationToken);
    }
}
```

- [ ] **Step 3: Write PriceDeletedHandler**

```csharp
// Application/Features/Pricing/Events/PriceDeletedHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Prices.Events;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Events;

internal sealed class PriceDeletedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<PriceDeletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PriceDeletedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.ActivePrice(evt.VariantId), cancellationToken);
    }
}
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build AE.Market.slnx`
Expected: Build succeeds

- [ ] **Step 5: Commit**

```bash
git add Application/Features/Pricing/Events/PriceCreatedHandler.cs Application/Features/Pricing/Events/PriceUpdatedHandler.cs Application/Features/Pricing/Events/PriceDeletedHandler.cs
git commit -m "feat(pricing): add cache invalidation handlers for price events"
```

---

### Task 2: OrderCancelled Handler (Stock Release)

**Files:**
- Create: `Application/Features/Orders/Events/OrderCancelledHandler.cs`

**Interfaces:**
- Consumes: `IReadRepository<Order>` (from `Application/Common/Interfaces/IReadRepository.cs`), `IRepository<ProductVariant>` (from `Application/Common/Interfaces/IRepository.cs`), `ICacheService`, `OrderByIdSpec` (from `Application/Features/Orders/Specs/OrderSpecs.cs`), `CacheKeys.Order(Guid)` (from `Application/Features/Orders/CacheKeys.cs`)
- Produces: Handler that releases reserved stock on order cancellation

- [ ] **Step 1: Write OrderCancelledHandler**

```csharp
// Application/Features/Orders/Events/OrderCancelledHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Orders.Specs;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Aggregates.Orders.Events;
using AE.Market.Application.Services;
using MediatR;

namespace AE.Market.Application.Features.Orders.Events;

internal sealed class OrderCancelledHandler(
    IReadRepository<Order> orderRepo,
    IRepository<ProductVariant> variantRepo,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<OrderCancelledDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCancelledDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var order = await orderRepo.FirstOrDefaultAsync(
            new OrderByIdSpec(evt.OrderId), cancellationToken);

        if (order is null)
            return;

        foreach (var item in order.Items)
        {
            var variant = await variantRepo.GetByIdWithTrackingAsync(item.VariantId, cancellationToken);
            if (variant is null)
                continue;

            variant.ReleaseStock(item.Quantity);
        }

        await cache.RemoveAsync(CacheKeys.Order(evt.OrderId), cancellationToken);
    }
}
```

- [ ] **Step 2: Build and verify**

Run: `dotnet build AE.Market.slnx`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add Application/Features/Orders/Events/OrderCancelledHandler.cs
git commit -m "feat(orders): add stock release handler for order cancellation"
```

---

### Task 3: Inventory Event Handlers (Cache + Sync)

**Files:**
- Create: `Application/Features/Inventory/Events/InventoryCreatedHandler.cs`
- Create: `Application/Features/Inventory/Events/OutOfStockHandler.cs`
- Create: `Application/Features/Inventory/Events/StockReservedHandler.cs`
- Create: `Application/Features/Inventory/Events/StockReleasedHandler.cs`
- Create: `Application/Features/Inventory/Events/LowStockHandler.cs`

**Interfaces:**
- Consumes: `ICacheService`, `IRepository<ProductVariant>`, `CacheKeys.Stock(Guid)`, `CacheKeys.LowStockReport` (from `Application/Features/Inventory/CacheKeys.cs`), `ProductByVariantIdSpec` (from `Application/Features/Catalog/Specs/ProductsByVariantIdSpec.cs` — NOTE: check actual filename)
- Produces: Five handlers for inventory events

- [ ] **Step 1: Write InventoryCreatedHandler**

```csharp
// Application/Features/Inventory/Events/InventoryCreatedHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class InventoryCreatedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<InventoryCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<InventoryCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.Stock(evt.VariantId), cancellationToken);
    }
}
```

- [ ] **Step 2: Write OutOfStockHandler**

```csharp
// Application/Features/Inventory/Events/OutOfStockHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class OutOfStockHandler(
    IRepository<ProductVariant> variantRepo,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<OutOfStockDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OutOfStockDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var variant = await variantRepo.GetByIdWithTrackingAsync(evt.VariantId, cancellationToken);
        if (variant is not null)
        {
            variant.Deactivate();
        }

        await cache.RemoveAsync(CacheKeys.Stock(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.LowStockReport, cancellationToken);
    }
}
```

- [ ] **Step 3: Write StockReservedHandler**

```csharp
// Application/Features/Inventory/Events/StockReservedHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class StockReservedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<StockReservedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<StockReservedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.Stock(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.LowStockReport, cancellationToken);
    }
}
```

- [ ] **Step 4: Write StockReleasedHandler**

```csharp
// Application/Features/Inventory/Events/StockReleasedHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class StockReleasedHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<StockReleasedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<StockReleasedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        await cache.RemoveAsync(CacheKeys.Stock(evt.VariantId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.LowStockReport, cancellationToken);
    }
}
```

- [ ] **Step 5: Write LowStockHandler**

```csharp
// Application/Features/Inventory/Events/LowStockHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Inventory.Events;
using MediatR;

namespace AE.Market.Application.Features.Inventory.Events;

internal sealed class LowStockHandler(
    ICacheService cache
) : INotificationHandler<DomainEventNotification<LowStockThresholdReachedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LowStockThresholdReachedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CacheKeys.LowStockReport, cancellationToken);
    }
}
```

- [ ] **Step 6: Build and verify**

Run: `dotnet build AE.Market.slnx`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add Application/Features/Inventory/Events/
git commit -m "feat(inventory): add event handlers for cache invalidation and out-of-stock sync"
```

---

### Task 4: Variant Activation Handlers (Product Sync)

**Files:**
- Create: `Application/Features/Catalog/Events/VariantActivatedHandler.cs`
- Create: `Application/Features/Catalog/Events/VariantActivatedHandler.cs`

**Interfaces:**
- Consumes: `IRepository<Product>`, `ICacheService`, `CacheKeys.ProductById(Guid)`, `CacheKeys.ProductsList` (from `Application/Features/Catalog/CacheKeys.cs`)
- Produces: Two handlers that sync variant activation to parent product

- [ ] **Step 1: Write VariantActivatedHandler**

```csharp
// Application/Features/Catalog/Events/VariantActivatedHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events;

internal sealed class VariantActivatedHandler(
    IRepository<Product> productRepo,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<VariantActivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantActivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var product = await productRepo.GetByIdWithTrackingAsync(evt.ProductId, cancellationToken);
        if (product is null)
            return;

        if (product.Status != ProductStatus.Active)
        {
            product.Activate();
        }

        await cache.RemoveAsync(CacheKeys.ProductById(evt.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList, cancellationToken);
    }
}
```

- [ ] **Step 2: Write VariantDeactivatedHandler**

```csharp
// Application/Features/Catalog/Events/VariantDeactivatedHandler.cs
using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Events;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Events;

internal sealed class VariantDeactivatedHandler(
    IRepository<Product> productRepo,
    ICacheService cache
) : INotificationHandler<DomainEventNotification<VariantDeactivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<VariantDeactivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var product = await productRepo.GetBySpecWithTrackingAsync(
            new Application.Features.Catalog.Specs.ProductByVariantIdSpec(evt.VariantId),
            cancellationToken);

        if (product is null)
            return;

        var hasActiveVariant = product.Variants.Any(v =>
            v.Status == ProductStatus.Active && !v.IsDeleted);

        if (!hasActiveVariant && product.Status == ProductStatus.Active)
        {
            product.Deactivate();
        }

        await cache.RemoveAsync(CacheKeys.ProductById(evt.ProductId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.ProductsList, cancellationToken);
    }
}
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build AE.Market.slnx`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add Application/Features/Catalog/Events/VariantActivatedHandler.cs Application/Features/Catalog/Events/VariantDeactivatedHandler.cs
git commit -m "feat(catalog): add variant activation sync handlers"
```

---

### Task 5: Final Verification

- [ ] **Step 1: Run full domain test suite**

Run: `dotnet test tests/AE.Market.Domain.Tests/`
Expected: All 575 tests PASS

- [ ] **Step 2: Run architecture tests**

Run: `dotnet test AE.Market.ArchitectureTests/AE.Market.ArchitectureTests.csproj`
Expected: 53 pass, 2 fail (pre-existing)

- [ ] **Step 3: Run build**

Run: `dotnet build AE.Market.slnx`
Expected: 0 errors

- [ ] **Step 4: Verify all events have handlers**

Grep for `IDomainEvent` implementations in Domain/, then check each has a handler in Application/. Expected: 0 unhandled events.

- [ ] **Step 5: Final commit (if any fixes needed)**
