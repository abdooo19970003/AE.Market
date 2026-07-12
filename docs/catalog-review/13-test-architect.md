# Test Architect — Catalog Feature Test Coverage Review

## Summary

| Severity | Count |
|----------|-------|
| Critical | 3 |
| Major | 5 |
| Minor | 3 |
| Info | 3 |

---

## Critical

### 1. `ProductTaxCode` — zero test coverage
`Domain/Aggregates/Catalog/Products/ProductTaxCode.cs` — Aggregate root with `Create()`, `UpdateDetails()`, `Delete()`, `Restore()`, and 3 domain events. No test file exists.

**Fix:** Create `ProductTaxCodeTests.cs`.

### 2. `GroupUnit` — zero test coverage
`Domain/Aggregates/Catalog/Units/GroupUnit.cs` — Aggregate root with `Create()`, `AddUnit()`, `RemoveUnit()`, `Rename()`, `Delete()`, and 2 domain events. No test file exists.

**Fix:** Create `GroupUnitTests.cs`.

### 3. `Unit` — zero test coverage
`Domain/Aggregates/Catalog/Units/Unit.cs` — Entity with `Create()`, `UpdateExchangeRate()`, `SetAsBaseUnit()`. No test file exists.

**Fix:** Create `UnitTests.cs`.

---

## Major

### 4. Pricing & stock domain logic untested on `ProductVariant`

| Operation | File:Line | Missing Tests |
|---|---|---|
| `SetOrUpdateSellingPrice` | `ProductVariant.cs:107-120` | Negative price, price > list price, VariantPriceChangedDomainEvent |
| `SetOrUpdateListPrice` | `ProductVariant.cs:122-131` | Entirely untested |
| `SetQuantity` | `ProductVariant.cs:133-144` | Negative quantity, domain event |
| `AdjustStock` | `ProductVariant.cs:146-157` | Entirely untested |
| `ReserveStock` | `ProductVariant.cs:159-169` | Entirely untested |
| `ReleaseStock` | `ProductVariant.cs:171-181` | Entirely untested |
| `AvailableQuantity` | `ProductVariant.cs:17` | Computed property untested |
| `Activate()`/`Deactivate()` | `ProductVariant.cs:48-60` | Entirely untested |

### 5. No architecture rule for Catalog mutation permissions
`ApiTests.cs:228-275` — `AdminEndpoints_Should_HaveMutateUsersPermission` only checks routes containing "users". No equivalent for Catalog endpoints.

### 6. No controller endpoint pattern rule
No architecture test verifies all controllers use `[Route("api/[controller]")]`, `[ApiController]`, primary constructor injection patterns.

### 7. Zero Catalog integration tests
`tests/AE.Market.Integration.Tests/` — Only `AuthIntegrationTests.cs` exists. 33 Catalog API endpoints have zero integration test coverage.

---

## Minor

### 8. Remaining untested entity operations
- **Brand**: `UpdateSlug()`, `Delete()`, `SetOrUpdateMetaFields()`, no event assertions
- **Category**: `UpdateSlug()`, `SetOrUpdateMetaFields()`, `CategoryUrl` computed property
- **Product**: `UpdateSlug()`, `SetShortDescription()`, `SetLongDescription()`, `SetOrUpdateMetaFields()`, `Restore()`, computed properties
- **ShippingDimensions**: No validation tests (negative weights, zero values)

### 9. `CategoryTests.cs` uses reflection to set private fields
`CategoryTests.cs:125-131` — `GetField("_subCategories")` is fragile.

### 10. `Placeholder.cs` files mask incomplete feature folders
Architecture tests pass Placeholder stubs as real implementations. Cannot distinguish stubs from real code.

---

## Info

### 11. Recommendation: Create missing domain tests first, then integration tests
Priority order: aggregate roots → stock/pricing logic → remaining entity operations → Catalog integration tests.

### 12. All existing tests follow good patterns
FluentAssertions, isolated test instances, Arrange/Act/Assert comments, domain event assertions.

### 13. Create architecture test for MutationProducts permission on Catalog endpoints
Pattern already exists for `MutateUsers`. Replicate for Catalog.
