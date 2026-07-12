# Domain Expert Evaluation: Pricing, Inventory, Stock, Units & Tax

## Executive Summary

The AE.Market Catalog aggregate has a solid foundation for variant-level pricing and stock management, with good concurrency awareness via `RowVersion` and clean domain event coverage for price and stock changes. However, the implementation is currently only suitable for **single-currency, single-warehouse, single-jurisdiction** scenarios typical of a minimum viable product. The pricing model lacks tiered/discounted/scheduled pricing, customer-group targeting, and currency support. The stock model has no warehouse-level tracking, low-stock alerts, or backorder management at the variant level. The tax system is purely a classification (tax code) with no rate tables or jurisdiction mapping. The unit conversion system has hardcoded temperature formulas only and lacks a general-purpose conversion engine, SI/Imperial system support, or unit-pricing capabilities. The system is well-architected for extension but requires significant expansion to handle real-world e-commerce complexity.

---

## PROS

**Stock Operations (reserve/release/adjust) are well-modeled**
- `ProductVariant.cs:136-156` -- Clean CQS-style stock methods: `AdjustStock(int delta)`, `ReserveStock(int quantity)`, `ReleaseStock(int quantity)`, `SetQuantity(int quantity)`. The distinction between `StockQuantity`, `ReservedQuantity`, and the computed `AvailableQuantity` (line 20) is exactly right for an e-commerce reservation system.
- `Product.cs:297-340` -- The Product aggregate root properly delegates stock operations to variants with consistent null-checking and domain event firing.

**Concurrency protection via RowVersion**
- `ProductVariant.cs:21` -- A `byte[] RowVersion` with `IsRowVersion()` in `ProductVariantConfiguration.cs:24` provides optimistic concurrency for stock and price updates, critical for preventing overselling in high-traffic scenarios.

**Domain events are comprehensive for the core operations that exist**
- `VariantPriceChangedDomainEvent` (carries `OldPrice`, `NewPrice`) and `VariantStockAdjustedDomainEvent` (carries `OldQuantity`, `NewQuantity`, `Delta`) are well-structured with enough context for downstream handlers.
- Events are consumed: `VariantPriceChangedEventHandler` and `VariantStockAdjustedEventHandler` invalidate cache; `VariantStockAdjustedAlertHandler` logs warnings for out-of-stock conditions -- a good pattern for extensibility.

**Unit conversion grouping is a good abstraction**
- `GroupUnit.cs:63-93` -- The `Convert(decimal value, Unit from, Unit to)` method validates intra-group conversion, normalizes via a base unit, and supports limited formula-based conversion.
- `GroupUnit.cs:14` -- `BaseUnitAbbreviation` computed property is a convenient accessor.
- `Unit.cs:12-13` -- `IsBaseUnit` and `ExchangeRateToBaseUnit` correctly model the standard approach of normalizing all units to a single base unit within a group for conversion.

**Tax code seeder is impressively comprehensive for classification**
- `ProductTaxCodeSeeder.cs:10-683` -- 60+ seed tax codes covering physical goods, digital goods, services, food, apparel, electronics, baby products, shipping, warranties, etc., following the `txcd_*` classification scheme compatible with major tax calculation engines (e.g., Avalara, Vertex). This is an excellent data foundation.

**Pricing is at the variant level (correct)**
- `ProductVariant.cs:16-17` -- `SalePrice` and `ListPrice` are on the variant entity, which is the correct granularity for configurable products where each variant (e.g., size/color) can have its own price.

**Backorder awareness exists at the Product level**
- `Product.cs:29-30` and `Product.cs:267-272` -- `AllowBackOrder` and `BackOrderLimit` are present, with a dedicated `SetAllowBackOrder` method, acknowledging that backorders are a real e-commerce concern.

**Shipping dimensions value object**
- `ShippingDimensions.cs` -- Weight, length, height, width (metric units) exist as a value object, providing the data needed for shipping rate calculation integration.

---

## GAPS

**GAP-1: No tiered/volume/discounted pricing** -- **Severity: HIGH**
- `ProductVariant.cs:16-17` -- Only `SalePrice` and `ListPrice` exist. There are no concepts for:
  - Volume/quantity breaks (e.g., $10 each for 1-5, $8 each for 6+)
  - Time-limited sale pricing (e.g., "Black Friday price valid Nov 25-30")
  - Customer-group/customer-specific pricing (e.g., "wholesale price vs retail price")
  - Minimum advertised price (MAP) enforcement
  - Cost/wholesale price tracking (for margin calculations)
- **Impact**: Cannot implement common B2B/promotional pricing strategies without significant rework.

**GAP-2: No multi-currency support** -- **Severity: HIGH**
- All price fields (`SalePrice`, `ListPrice`) are plain `decimal` with no currency context. There is no `Currency` value object, no exchange rate table, no price-by-currency structure.
- **Impact**: Cannot operate an international storefront accepting multiple currencies.

**GAP-3: No warehouse/location-level inventory** -- **Severity: HIGH**
- `ProductVariant.cs:18-20` -- `StockQuantity`, `ReservedQuantity`, and `AvailableQuantity` are single scalar values. There is no concept of:
  - Multiple warehouses or fulfillment centers
  - Location-specific stock levels
  - Stock transfers between locations
  - Bin/rack location tracking
- **Impact**: Cannot support multi-warehouse fulfillment, dropshipping, or brick-and-mortar + online inventory management.

**GAP-4: No tax rate tables or jurisdiction mapping** -- **Severity: HIGH**
- `ProductTaxCode.cs` -- The entity is purely a classification label (Code, Type, Name, Description). There are no:
  - Tax rate tables (percentage amounts per jurisdiction)
  - Jurisdiction/nexus mapping (which tax codes apply in which states/countries)
  - Tax-inclusive vs. tax-exclusive pricing flags
  - Tax exemptions (customer-level, product-level, or order-level)
  - Tax reporting codes or breakdowns (e.g., state vs. county vs. city portions)
- **Impact**: The tax code classification is useful for integration with third-party tax engines (Avalara, TaxJar), but the system itself cannot calculate or report taxes.

**GAP-5: No low-stock thresholds or automated reordering** -- **Severity: MEDIUM**
- No field exists on `ProductVariant` for `LowStockThreshold`, `ReorderPoint`, or `ReorderQuantity`. The only stock alert is the `VariantStockAdjustedAlertHandler` logging a warning when `NewQuantity <= 0`.
- **Impact**: Merchants cannot configure proactive low-stock notifications; they only learn of stockouts after the fact.

**GAP-6: Backorder support is at Product level, not Variant level** -- **Severity: MEDIUM**
- `Product.cs:29-30` -- `AllowBackOrder` and `BackOrderLimit` are on `Product`. For configurable products, some variants may be on backorder while others are not.
- **Impact**: Cannot granularly control backorders per variant (e.g., size "Large" is on backorder but "Medium" is in stock).

**GAP-7: No unit pricing (price per unit of measure)** -- **Severity: MEDIUM**
- The `Unit` and `GroupUnit` system can convert between units (e.g., kg to lb), but there is no concept of "unit price" -- the price displayed per kg, per liter, per meter. In grocery/commodity e-commerce, displaying `$3.99/kg` alongside the item price is mandatory.
- **Impact**: Cannot support grocery, bulk, or commodity-style selling.

**GAP-8: FormulaType is hardcoded and incomplete** -- **Severity: MEDIUM**
- `FormulaType.cs` -- Only three values: `None`, `CelsiusToFahrenheit`, `FahrenheitToCelsius`. There is no mechanism for custom or general-purpose conversion formulas.
- The conversion logic in `GroupUnit.cs:75-93` uses `switch` on `FormulaType`, which is not extensible without code changes.
- Missing: Volume (gallon <-> liter), Length (inch <-> cm), Mass (lb <-> kg) as built-in formulas.
- **Impact**: Cannot support non-temperature unit types with non-linear conversion formulas. Adding a new formula type requires modifying the enum and the switch statement in two places.

**GAP-9: No price lists or scheduled pricing** -- **Severity: MEDIUM**
- No way to define named price lists (e.g., "Wholesale 2025", "Holiday Sale") with time-bound validity.
- `ProductVariant.cs:117-121` -- `SetOrUpdateSellingPrice` sets a single price with no effective date range.
- **Impact**: No support for seasonal promotions, flash sales, or B2B contract pricing without external scheduling logic.

**GAP-10: No inventory valuation or cost tracking** -- **Severity: LOW**
- No `CostPrice` or `UnitCost` field on `ProductVariant`. No concept of inventory valuation method (FIFO, LIFO, weighted average).
- **Impact**: Cannot calculate gross margin, COGS, or inventory value from this system alone.

**GAP-11: No stock movement history** -- **Severity: LOW**
- While `VariantStockAdjustedDomainEvent` is fired and goes through the outbox, there is no dedicated stock movement log or audit table that captures each stock event with reason code (e.g., "purchase order received", "customer order shipped", "adjustment - damaged").
- **Impact**: Auditing inventory changes requires replaying events; no queryable stock movement history exists.

**GAP-12: No physical inventory / cycle count support** -- **Severity: LOW**
- No mechanism to record physical inventory counts, flag discrepancies, or freeze stock during counting.
- **Impact**: No support for periodic inventory reconciliation workflows.

---

## ISSUES

**ISSUE-1: `ListPrice` exists but is completely inaccessible via API** -- **Severity: HIGH**
- `ProductVariant.cs:17` defines `ListPrice`, and `SetOrUpdateListPrice` exists (line 123-127).
- However:
  - `UpdateVariantPricingCommand.cs:6-10` only accepts `SalePrice`, not `ListPrice`.
  - `VariantDto.cs:10-13` does **not** include `ListPrice` -- only `SalePrice`, `StockQuantity`, `ReservedQuantity`, `AvailableQuantity`.
  - `ProductVariantConfiguration.cs:15-24` does **not** configure the `ListPrice` column.
- This means `ListPrice` is never persisted to the database and never exposed over the API. It is effectively **dead code**. If a developer were to call `SetOrUpdateListPrice`, the value would be lost on the next read.
- **File references**: `ProductVariant.cs:17,123-127`, `UpdateVariantPricingCommand.cs:9`, `VariantDto.cs:10`, `ProductVariantConfiguration.cs:22` (no ListPrice mapping).

**ISSUE-2: `ReleaseStock` silently clamps to zero instead of throwing** -- **Severity: MEDIUM**
- `ProductVariant.cs:152-156`:
  ```csharp
  internal void ReleaseStock(int quantity)
  {
      ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
      UpdateLastModified();
  }
  ```
  If a bug causes `ReleaseStock` to be called with a quantity exceeding `ReservedQuantity`, the operation silently succeeds and sets `ReservedQuantity` to 0 rather than throwing a domain exception. This can mask logic errors in the ordering pipeline (e.g., releasing the same stock twice, or releasing more than was reserved). Compare with `ReserveStock` (line 144-149) which throws `InsufficientStock` -- the inconsistency in error handling creates a potential debugging trap.
- Compare with `ReserveStock` which throws on insufficient available quantity (good), but `ReleaseStock` has no such validation.
- **Recommendation**: Should throw `DomainException` with code `Catalog.Variant.ReleaseExceedsReserved` if `quantity > ReservedQuantity`.

**ISSUE-3: No domain events for Reserve/Release stock operations** -- **Severity: MEDIUM**
- `Product.cs:322-340` -- `ReserveVariantStock` and `ReleaseVariantStock` do not fire domain events. Only `AdjustVariantStock` (line 309) and `SetVariantQuantity` (line 297) fire `VariantStockAdjustedDomainEvent`.
- This means downstream handlers (cache invalidation, logging, analytics) are **not notified** when stock is reserved or released. The `VariantStockAdjustedAlertHandler` will not log reservations or releases, and the cache will not be invalidated.
- **File references**: `Product.cs:322-340`, `VariantStockAdjustedDomainEvent.cs`.

**ISSUE-4: `Product.SalePrice` computed property uses `SalePrice > 0` filter which excludes free products** -- **Severity: MEDIUM**
- `Product.cs:57`:
  ```csharp
  public decimal SalePrice => _variants.Where(v => v.IsActive && v.SalePrice > 0).Min(v => (decimal?)v.SalePrice) ?? 0m;
  ```
  The `v.SalePrice > 0` condition means that if all active variants have a price of 0 (free product), the computed `Product.SalePrice` returns 0. But it also means that if the lowest-priced active variant costs 0 and another costs $10, the product-level price will show as $10 (the next cheapest), not $0. This is semantically incorrect: a free variant's price is 0, and the product's minimum price should be 0.
- Additionally, sorting by price in `GetProductsListQueryHandler.cs:27` uses `p => p.SalePrice` which is an ignored computed column -- this EF Core query would fail at runtime or produce incorrect results since `SalePrice` is `Ignore()`d in the config.
- **File references**: `Product.cs:57-58`, `ProductConfiguration.cs:45`, `GetProductsListQueryHandler.cs:27`.

**ISSUE-5: Race condition window between load and save in stock operations** -- **Severity: MEDIUM**
- All stock command handlers (e.g., `ReserveVariantStockCommandHandler.cs:19-36`, `AdjustVariantStockCommandHandler.cs:19-41`) follow the pattern: load product with tracking, modify variant in memory, call `repo.Update(product)`. There is a gap between loading and saving where concurrent requests could read stale `AvailableQuantity` and both succeed in reserving the same stock.
- `RowVersion` on `ProductVariant` provides optimistic concurrency, but the handler does not retry on `DbUpdateConcurrencyException`. If the save fails due to a version conflict, the caller receives an unhandled exception rather than a graceful retry or failure response.
- **File references**: `ProductVariant.cs:21`, `ReserveVariantStockCommandHandler.cs:19-36`, `AdjustVariantStockCommandHandler.cs:19-41`.

**ISSUE-6: `UpdateVariantPricingCommandValidator` allows zero pricing but does not validate against negative prices** -- **Severity: LOW**
- `UpdateVariantPricingCommandValidator.cs:11`: `RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0)`.
- Not necessarily wrong (free products are valid), but a missing `GreaterThan(0)` validation on `ListPrice` (if it were wired up) would be anomalous -- a list price of 0 would suggest it's unused, but semantically a list price of 0 means "free" which contradicts the concept of a reference price.

**ISSUE-7: `AdjustVariantStock` allows setting stock below reserved quantity via `SetQuantity` but not via `AdjustStock`** -- **Severity: LOW**
- `ProductVariant.cs:129-134` (`SetQuantity`): Throws if `quantity < ReservedQuantity`.
- `ProductVariant.cs:136-142` (`AdjustStock`): Throws only if `newQuantity < 0`, but does not guard against `newQuantity < ReservedQuantity`. If you adjust stock by a negative delta that brings `StockQuantity` below `ReservedQuantity`, you end up with `AvailableQuantity` being negative.
- **File references**: `ProductVariant.cs:129-142`.

**ISSUE-8: Tax Code change on Product fires domain event but has no handler** -- **Severity: LOW**
- `Product.cs:147-153` -- `UpdateTaxCode` fires `ProductTaxCodeChangedDomainEvent(Id, oldTaxCodeId, newTaxCodeId)`.
- However, the event handler (`ProductTaxCodeChangedDomainEvent`) is not listed under any handler directory -- I did not find a `ProductTaxCodeChangedEventHandler`. The created/updated/deleted event handlers for `ProductTaxCode` entity are all no-ops (return `Task.CompletedTask`). So the tax code change event is fired but never acted upon (no cache invalidation, no recalculation, no logging).

---

## Recommendations (Top 5 Actionable Improvements)

**RECOMMENDATION 1: Fix the `ListPrice` dead code immediately (HIGH priority)**
- The `ListPrice` field on `ProductVariant` is configured to never be persisted (`ProductVariantConfiguration.cs` does not map it) and never exposed via API (`VariantDto.cs` excludes it, `UpdateVariantPricingCommand` does not include it). This is a clear integration gap.
- **Action**: (a) Add `ListPrice` column mapping to `ProductVariantConfiguration.cs` with `HasPrecision(18,4)`. (b) Add `ListPrice` to `VariantDto.cs`. (c) Either extend `UpdateVariantPricingCommand` to accept `decimal? ListPrice` or create a separate `SetVariantListPrice` command. (d) Wire up `SetOrUpdateListPrice` on the Product aggregate with its own domain event.

**RECOMMENDATION 2: Add domain events for Reserve/Release and fix the ReleaseStock safety check (HIGH priority)**
- `ReserveVariantStock` and `ReleaseVariantStock` on `Product.cs` do not fire any domain events, breaking downstream concerns (cache invalidation, analytics, logging).
- **Action**: (a) Fire `VariantStockReservedDomainEvent` and `VariantStockReleasedDomainEvent` from `Product.cs:322-340`. (b) Update `VariantStockAdjustedAlertHandler` to also listen for these events. (c) Fix `ReleaseStock` in `ProductVariant.cs:154` to throw `DomainException` when `quantity > ReservedQuantity` instead of silently clamping to zero.

**RECOMMENDATION 3: Add tiered pricing and price list infrastructure (MEDIUM priority)**
- The current single `SalePrice` + `ListPrice` model is insufficient for real-world commerce.
- **Action**: Introduce a `PriceTier` entity (or value object) with fields: `VariantId`, `Currency`, `MinQuantity` (nullable for base price), `MaxQuantity` (nullable), `Price`, `PriceListId`. A `PriceList` aggregate can group these with date ranges and customer group targeting. This replaces the ad-hoc single-price model and enables volume discounts, B2B pricing, and promotional pricing.

**RECOMMENDATION 4: Introduce warehouse-level inventory (MEDIUM priority)**
- The single scalar `StockQuantity` is a bottleneck for multi-location fulfillment.
- **Action**: Create a `WarehouseStock` entity: `Id`, `VariantId`, `WarehouseId`, `StockQuantity`, `ReservedQuantity`, `LowStockThreshold`, `RowVersion`. The `ProductVariant` aggregate can hold a collection of `WarehouseStock` and compute aggregate totals. `ReserveStock`/`ReleaseStock` operations should accept an optional `WarehouseId` parameter. This enables location-aware allocation and low-stock alerts per warehouse.

**RECOMMENDATION 5: Replace hardcoded `FormulaType` with a data-driven conversion system (MEDIUM priority)**
- The current `FormulaType` enum with inline `switch` statements in `GroupUnit.cs:75-93` is brittle and non-extensible.
- **Action**: Remove the `FormulaType` enum approach. Instead, add a `ConversionFormula` field (string) to `Unit` that stores a mathematical expression (e.g., `"x * 1000"` for kg to g). Use a simple expression parser/calculator library (or store two coefficients: `Factor` and `Offset`). This allows all linear conversions (mass, length, volume, temperature) to be data-driven without code changes. The `GroupUnit.Convert` method becomes a simple `value * from.Factor + from.Offset` then `(value - to.Offset) / to.Factor` pattern, supporting both multiplicative and additive conversions.
