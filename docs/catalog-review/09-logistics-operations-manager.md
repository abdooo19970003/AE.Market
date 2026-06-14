# Logistics Operations Manager — Inventory & Fulfillment Review

## Summary

| Severity | Count |
|----------|-------|
| Critical | 3 |
| Major | 4 |
| Minor | 4 |
| Info | 3 |

---

## Critical

### 1. `SetQuantity` ignores `ReservedQuantity` (negative available stock)
`ProductVariant.cs:133-144` — `SetQuantity(3)` when `StockQuantity=10, ReservedQuantity=5` produces `AvailableQuantity = -2`. The `PATCH .../stock` endpoint uses `SetQuantity`, making this trivially triggerable.

**Fix:** Guard with `if (quantity < ReservedQuantity) throw`.

### 2. No concurrency control on stock operations
`ProductVariantConfiguration.cs` — No `RowVersion`, `Timestamp`, or `ConcurrencyCheck`. Two parallel requests can both pass the guard and oversell. `BACKEND_PLAN.md` documents `row_version` as planned (line 663).

**Fix:** Add `byte[] RowVersion` with `IsRowVersion()` / `IsRowConcurrencyToken()`.

### 3. `ReservedQuantity` not persisted in EF configuration
`ProductVariantConfiguration.cs:22` — Only `StockQuantity` is configured. `ReservedQuantity` may be silently dropped or default to wrong column type.

**Fix:** Add `builder.Property(x => x.ReservedQuantity).HasDefaultValue(0)`.

---

## Major

### 4. `ReserveStock`/`ReleaseStock` have no command handlers or API endpoints
`ProductVariant.cs:159-181` — Dead code. No MediatR command, no handler, no controller endpoint, no tests.

**Fix:** Scaffold `ReserveVariantStockCommand` / `ReleaseVariantStockCommand` handlers and expose `POST .../variants/{variantId}/reserve` and `POST .../variants/{variantId}/release`.

### 5. No Order aggregate / order state machine
No `Domain/Aggregates/Orders/` exists. Reservation methods on variants imply they should be called by an order placement handler.

**Fix:** Implement `Order` aggregate with status machine and hook `OrderPlacedDomainEvent` → `ReserveStock`.

### 6. `VariantStockAdjustedDomainEvent` has no consumer
Event fires but nobody subscribes for low-stock alerts or fulfillment triggers.

**Fix:** Add `INotificationHandler<VariantStockAdjustedDomainEvent>` for alerts/fulfillment.

### 7. `AdjustStock` domain method has no handler/API
`ProductVariant.cs:146-157` — Delta-based adjustment is unused. Only `SetQuantity` (absolute) is wired.

**Fix:** Create `AdjustVariantStockCommand(int Delta)` handler and API endpoint.

---

## Minor

### 8. `AvailableQuantity` not exposed in DTOs
`VariantDto.cs:11` — API consumers cannot see how many units are available.

### 9. Product variant has no unit-of-measure linkage
`ProductVariant.cs:8-18` — No `UnitId` or `GroupUnitId`. All stock implicitly counted in "pieces".

### 10. No warehouse/location tracking
No `Warehouse` aggregate, no `StockEntry` per location. `BACKEND_PLAN.md` Sprint 6 describes this.

### 11. `SetQuantity` duplicates event logic with `AdjustStock`
Lines 133-144 and 146-157 repeat the event-raising pattern.

---

## Info

### 12. `AllowBackOrder`/`BackOrderLimit` never checked during reservation
`ReserveStock` does not bypass `AvailableQuantity` guard when backorder is allowed.

### 13. No shipping API integration layer
`ShippingDimensions` exists but no carrier integration.

### 14. No tests for stock operations
`ProductVariantTests.cs:106-127` tests only `SetQuantity(50)`. Zero tests for `AdjustStock`, `ReserveStock`, `ReleaseStock`, boundary conditions.
