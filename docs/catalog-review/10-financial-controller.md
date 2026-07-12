# Financial Controller — GAAP/IFRS & Tax Compliance Audit

## Summary

| Severity | Count |
|----------|-------|
| Critical | 2 |
| Major | 4 |
| Minor | 3 |

---

## Critical

### 1. `ListPrice` NOT persisted to database
`ProductVariantConfiguration.cs:21` — Only `SalePrice` is mapped with `HasPrecision(18,4)`. `ListPrice` has no column configuration. EF Core's Npgsql default (`numeric(0,0)`) silently truncates values — a direct books-to-ledger reconciliation risk.

**Fix:** Add `builder.Property(x => x.ListPrice).HasPrecision(18, 4)`.

### 2. `SetOrUpdateListPrice` emits NO domain event
`ProductVariant.cs:122-131` — Changing a list/reference price is a pricing disclosure event, yet no `VariantListPriceChangedDomainEvent` exists and no event is fired.

**Fix:** Create `VariantListPriceChangedDomainEvent(OldListPrice, NewListPrice)` and fire it from `SetOrUpdateListPrice`.

---

## Major

### 3. `Product.UpdateTaxCode()` has NO domain event
`Product.cs:129-133` — Changing tax code is a tax compliance event. No audit trail exists.

**Fix:** Add `AddDomainEvent(new ProductTaxCodeChangedDomainEvent(Id, OldTaxCodeId, NewTaxCodeId))`.

### 4. `VariantDto` omits `ListPrice`
`VariantDto.cs:10` — API consumers cannot read the list/reference price.

**Fix:** Add `public decimal ListPrice { get; set; }` to `VariantDto`.

### 5. No revenue recognition hooks exist
No `Order`, `Fulfillment`, `Invoice`, or `Payment` aggregates. ASC 606 / IFRS 15 cannot be implemented.

**Fix:** (Out of scope) — Order/Invoice aggregates are prerequisites.

### 6. `ProductTaxCodeUpdatedDomainEvent` carries no diff
`ProductTaxCodeUpdatedEvent.cs:5` — Only carries `TaxCodeId`. Cannot reconstruct what changed.

**Fix:** Extend to include `OldCode`, `NewCode`, `OldType`, `NewType`.

---

## Minor

### 7. No `TaxExempt` flag on Product/ProductTaxCode
Neither entity has `IsTaxExempt` or `TaxExemptReason`. Seeder includes `"Nontaxable"` (`txcd_00000000`) but no per-item exemption support.

**Fix:** Add `bool IsTaxExempt` and `string? TaxExemptReason` to `Product`.

### 8. No nexus/jurisdiction model
No `TaxRate` table, no origin/destination-based sourcing rules. Comprehensive Avalara-style tax codes exist but cannot calculate rates.

**Fix:** Add `Jurisdiction` / `TaxRate` aggregate or prepare for Avalara/TaxJar integration.

### 9. `ProductTaxCodeDeletedDomainEvent` — soft-delete orphans products
`Product.cs:35` — `TaxCodeId` is a plain `Guid` with no FK constraint. Deleting a tax code breaks references.

**Fix:** Add `Restrict` delete behavior in config. Validate active products before delete.
