# Domain Guard — DDD Purity & Dependency Scan

## Summary

| Severity | Count |
|----------|-------|
| Critical | 3 |
| Major | 3 |
| Minor | 3 |
| Info | 4 |
| **Passing** | **14/14 checks** |

---

## ✅ Passing Checks

| Rule | Status |
|------|--------|
| No external package dependencies in `.csproj` | ✅ |
| No `using` to Application/Infrastructure/API | ✅ |
| No `using` to EF Core, MediatR, etc. | ✅ |
| Entity setters are `private` or `init` | ✅ |
| Collections as `IReadOnlyCollection<T>` | ✅ |
| Parameterless ctors on sealed entities are `private` | ✅ |
| Aggregate roots implement `IAggregateRoot` | ✅ |
| Cross-aggregate references use `Guid` not objects | ✅ |
| Domain events follow naming pattern | ✅ |
| Events raised in factory/command methods | ✅ |
| `BaseSpecification` doesn't reference EF types | ✅ |
| `Guard` is `internal static` | ✅ |
| Value objects are `record` types + `IValueObject` | ✅ (except Mony) |
| `static Create(...)` factory on entities | ✅ |

---

## Critical

### 1. `Address` implicit operator silently swallows validation
`Address.cs:35-41` — `implicit operator Address?(string value)` catches all exceptions and returns `null`, bypassing `Parse()` validation. Introduces `NullReferenceException` at call sites.

**Fix:** Remove try/catch. Expose explicit `TryParse` for nullable scenario.

### 2. Duplicate error system
`Exceptions/Auth.cs` & `Exceptions/Profile.cs` (throw `DomainException`) AND `Aggregates/Auth/Errors/AuthErrors.cs` & `Aggregates/Auth/Errors/ProfileErrors.cs` (return `Error` records). Value objects throw exceptions while Application consumes `Result<Error>`.

**Fix:** Consolidate: remove `Exceptions/Auth.cs` and `Exceptions/Profile.cs`. Migrate value object validation to use errors or domain exceptions only for true invariants.

### 3. `Mony` value object violates all Value Object rules
`Mony.cs:8-43` — Public constructor (not `private`), no `static Create(...)` factory, no input validation, `init` setters allow post-construction mutation. Does not implement required `IValueObject` pattern.

**Fix:** Make constructor `private`, add `static Create(decimal value, string currencyCode)` with validation (value >= 0, ISO currency code), remove `init` setters.

---

## Major

### 4. Namespace inconsistency — Auth uses block-scoped, Catalog uses file-scoped
All Auth files use `namespace X.Y { }`. Most Catalog files use `namespace X.Y;`. Even within Catalog, 5 files use block-scoped.

**Fix:** Normalize to file-scoped (preferred per AGENTS.md).

### 5. Child entities raise domain events directly
`ProductVariant.cs:119,143,156` — `AddDomainEvent(...)` called on child entity instance. Event dispatcher may miss these if it only inspects the aggregate root.

**Fix:** Either (a) propagate events up to `Product` aggregate root, or (b) ensure `SaveChangesInterceptor` traverses full entity graph.

### 6. Aggregate root methods accept raw child entities
`Product.RemoveVariant(ProductVariant)`, `Product.RemoveImage(ProductImage)`, etc. — Allow passing detached/unowned entities.

**Fix:** Accept identifiers (`Guid variantId`) and look up child internally.

---

## Minor

### 7. `VariantAttributeValue` declared `partial` with no other part
### 8. `ProductTaxCode` properties declared after methods
### 9. `UserProfile` constructor implicitly converts strings to `Name`

---

## Info

### 10. Typo in `ProfileErrors.cs:32` — "letter" should be "digits"
### 11. Multiple typos in `Mony.cs:7` — "Multibe" → "Multiple", "senario" → "scenario"
### 12. `DomainErrors.cs` is empty partial class
### 13. `BaseEntity.AddDomainEvent()` is `protected internal` — weakens child entity encapsulation
