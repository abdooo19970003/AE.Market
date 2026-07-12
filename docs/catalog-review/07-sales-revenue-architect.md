# Sales Revenue Architect — Catalog Pricing Review

## Summary

| Severity | Count |
|----------|-------|
| Critical | 2 |
| Major | 6 |
| Minor | 2 |
| Info | 1 |

---

## Critical

### 1. ListPrice changes fire no domain event
`ProductVariant.cs:122-131` — `SetOrUpdateListPrice()` mutates `ListPrice` silently. No `VariantListPriceChangedDomainEvent` is raised. List price adjustments affect perceived discount depth and are invisible to audit.

**Fix:** Add `AddDomainEvent(new VariantListPriceChangedDomainEvent(...))` inside `SetOrUpdateListPrice`, or introduce a dedicated `VariantListPriceChangedDomainEvent`.

### 2. No list price update command in Application layer
`UpdateVariantPricingCommand.cs:9` — The command only accepts `SalePrice`. There is no way to update `ListPrice` through the API.

**Fix:** Add a `ListPrice` field to `UpdateVariantPricingCommand`, or create a dedicated `UpdateVariantListPriceCommand`. The handler must call both `SetOrUpdateListPrice` (first) and `SetOrUpdateSellingPrice` (second) to maintain invariants.

---

## Major

### 3. `SetOrUpdateSellingPrice` invariant bypassed when `ListPrice == 0`
`ProductVariant.cs:111` — If `ListPrice` is the default `0m`, the guard `ListPrice > 0 && price > ListPrice` evaluates to `false`, so any arbitrary `SalePrice` passes.

**Fix:** Require `ListPrice` to be set first (throw if `ListPrice <= 0`), or treat `ListPrice == 0` as "unset" and still validate `price > 0`.

### 4. `SetOrUpdateSellingPrice` / `SetOrUpdateListPrice` throw exceptions
`ProductVariant.cs:107-131` — Violates project convention of returning `Result` / `Result<T>`. Pipeline bypasses `TransactionBehavior`.

**Fix:** Return `Result` from these methods. Callers check `.IsFailure` and propagate.

### 5. No price history / audit table
`VariantPriceChangedEventHandler.cs:12-19` — Event is only consumed for cache invalidation. Never persisted for time-travel queries.

**Fix:** Create a `price_history` table and write an outbox handler that inserts a row.

### 6. AddProductVariant sets no pricing
`AddProductVariantCommand.cs:7-9`, `Product.cs:153-160` — Variants created with `SalePrice = 0`, `ListPrice = 0`. Requires follow-up command.

**Fix:** Add optional `salePrice` / `listPrice` params to `Product.AddVariant()` and `AddProductVariantCommand`.

### 7. No currency attached to prices
`ProductVariant.cs:13-14` — `SalePrice`/`ListPrice` are raw `decimal`. `Mony` value object exists but is unused.

**Fix:** Replace `decimal` with `Mony` or add `CurrencyCode` column defaulting to `'USD'`.

### 8. No discount/promotion hooks
Entire catalog domain — No `DiscountedPrice`, no discount rules, no `PriceListItem` entity.

**Fix:** Add `DiscountedPrice` property and at minimum a `PriceList` aggregate or discount strategy abstraction.

---

## Minor

### 9. `Product.SalePrice` ignores inactive variants
`Product.cs:17` — Computed `Min(v => v.SalePrice)` includes deactivated variants.

**Fix:** Filter `.Where(v => v.IsActive)` before taking min.

### 10. `ListPrice` missing EF precision config
`ProductVariantConfiguration.cs:21-22` — `SalePrice` has `HasPrecision(18, 4)` but `ListPrice` does not.

**Fix:** Add `builder.Property(x => x.ListPrice).HasPrecision(18, 4)`.

---

## Info

### 11. Negative price via implicit cast
`Mony.cs:37` — `implicit operator decimal(Mony)` allows negative values in arithmetic without validation.

**Fix:** Avoid implicit operators. Use explicit validation in constructors.
