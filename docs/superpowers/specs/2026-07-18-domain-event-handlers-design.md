# Domain Event Handlers Design

**Date:** 2026-07-18
**Status:** Approved
**Scope:** 11 unhandled domain events across 4 aggregates

## Problem

82 domain events exist in the codebase. 71 have handlers. 11 do not. This means:
- Price changes don't invalidate cached active prices
- Order cancellation doesn't release reserved stock
- Inventory events (out-of-stock, stock reserved/released) don't update cache or sync to Catalog
- Variant activation/deactivation doesn't propagate to parent product status

## Design

### Group 1: Prices — Cache Invalidation (3 handlers)

**Events:** `PriceCreatedDomainEvent`, `PriceUpdatedDomainEvent`, `PriceDeletedDomainEvent`
**Files:** `Application/Features/Pricing/Events/PriceCreatedHandler.cs`, `PriceUpdatedHandler.cs`, `PriceDeletedHandler.cs`

All three follow the same pattern:
- Inject `ICacheService`
- On handle: `RemoveAsync(CacheKeys.ActivePrice(evt.VariantId, null))`
- Rationale: The event carries `PriceId` and `VariantId` but not `marketplaceId`. We flush the global key `price-active-{variantId}:`. Marketplace-specific keys (`price-active-{variantId}:{marketplaceId}`) expire via their 10-minute TTL.
- After a price is created, updated, or deleted, the cached "which price is active" answer may be wrong.

### Group 2: Orders — Stock Release (1 handler)

**Event:** `OrderCancelledDomainEvent(OrderId, UserId)`
**File:** `Application/Features/Orders/Events/OrderCancelledHandler.cs`

- Inject `IReadRepository<Order>`, `IRepository<ProductVariant>`
- Load order via `OrderByIdSpec(evt.OrderId)` (includes Items)
- Validate order status is `Cancelled` (already set by `Order.Cancel()`)
- For each order item: `variant.ReleaseStock(item.Quantity)` — mirrors `OrderPlacedHandler` which calls `variant.ReserveStock(quantity)`
- Flush `CacheKeys.Order(evt.OrderId)`
- Note: Stock reservation lives on `ProductVariant` (not `InventoryItem`), so release must also target `ProductVariant`

### Group 3: Inventory — Cross-Aggregate Sync + Cache (5 handlers)

**File:** `Application/Features/Inventory/Events/`

| Handler | Event | Behavior |
|---------|-------|----------|
| `InventoryCreatedHandler` | `InventoryCreatedDomainEvent(InventoryItemId, VariantId)` | Flush `inventory-stock-{variantId}`. No cross-aggregate sync needed — variant stock is managed separately from inventory tracking. |
| `OutOfStockHandler` | `OutOfStockDomainEvent(InventoryItemId, VariantId)` | Load variant → `variant.Deactivate()`. Flush `inventory-stock-{variantId}` + `inventory-lowstock-report`. |
| `StockReservedHandler` | `StockReservedDomainEvent(InventoryItemId, VariantId, Qty, NewTotal)` | Flush `inventory-stock-{variantId}` + `inventory-lowstock-report`. |
| `StockReleasedHandler` | `StockReleasedDomainEvent(InventoryItemId, VariantId, Qty, NewTotal)` | Same cache flush. |
| `LowStockHandler` | `LowStockThresholdReachedDomainEvent(InventoryItemId, VariantId, CurrentStock, Threshold)` | Flush `inventory-lowstock-report` only. Stock key stays valid (item exists, just low). |

### Group 4: Catalog Variants — Activation Sync (2 handlers)

**File:** `Application/Features/Catalog/Events/`

| Handler | Event | Behavior |
|---------|-------|----------|
| `VariantActivatedHandler` | `VariantActivatedDomainEvent(ProductId, VariantId)` | Load product → if product status is not Active, activate it. Flush `product-{ProductId}` + `products-list`. |
| `VariantDeactivatedHandler` | `VariantDeactivatedDomainEvent(ProductId, VariantId)` | Load product → check if any sibling variant is still active (`product.Variants.Any(v => v.Status == Active && !v.IsDeleted)`) → if none active, deactivate product. Flush `product-{ProductId}` + `products-list`. |

## Domain Changes

**None.** All 11 events are already defined and raised in their respective domain entities:
- `Price.cs` raises `PriceCreatedDomainEvent`, `PriceUpdatedDomainEvent`, `PriceDeletedDomainEvent`
- `Order.cs` raises `OrderCancelledDomainEvent`
- `InventoryItem.cs` raises `InventoryCreatedDomainEvent`, `OutOfStockDomainEvent`, `StockReservedDomainEvent`, `StockReleasedDomainEvent`, `LowStockThresholdReachedDomainEvent`
- `Product.cs` raises `VariantActivatedDomainEvent`, `VariantDeactivatedDomainEvent`

## Files to Create (11)

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

## Patterns to Follow

- All handlers are `internal sealed class`
- All implement `INotificationHandler<DomainEventNotification<TEvent>>`
- All use primary constructors for DI
- Cache invalidation via `ICacheService.RemoveAsync()`
- Cross-aggregate sync via `IRepository<T>.GetBySpecWithTrackingAsync()` + entity method calls
- No domain logic in handlers — only cache flush and cross-aggregate sync

## Testing

- Domain tests: 575 existing should still pass (no domain changes)
- Unit tests per handler: mock `ICacheService`, `IRepository<T>`, verify correct cache keys removed and entity methods called
- Integration tests: not needed — handlers are simple cache/sync operations

## Out of Scope

- InventoryItem stock reservation wiring in PlaceOrder (separate feature)
- Out-of-stock notification emails/webhooks (future)
- Low-stock alert system (future)
- Search index updates (future — no Elasticsearch yet)
