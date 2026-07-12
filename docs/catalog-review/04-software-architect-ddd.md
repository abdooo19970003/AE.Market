# Software Architect Review — Domain-Driven Design

**Reviewer:** Elena — DDD purity, aggregate boundaries, tactical patterns

---

## Aggregate Boundaries and the Category/Product Split

Declaring **both `Category` and `Product` as aggregate roots** (`IAggregateRoot`) is architecturally sound and well-justified. A Category has its own lifecycle (independent creation, hierarchical restructuring, activation/deactivation) that does not depend on any product. A Product exists within a category but is purchased, inventoried, and versioned independently. The **weak reference via `CategoryId` (a GUID)** is exactly the right approach — DDD demands that aggregate roots reference each other only by identity, never by object reference. This preserves the transactional boundary: loading a `Product` never forces you to load its `Category`, and vice versa. The codebase correctly avoids navigation properties like `Product.Category`.

However, there is a **boundary problem with `GroupUnit` and `Unit`**. Both are `BaseEntity` subclasses, both are **not** marked `IAggregateRoot`, yet `GroupUnit` manages a collection of `Unit` entities (add/remove), exposes a `Create` factory, and has mutation methods. This entity is effectively functioning as an aggregate root without the marker. Furthermore, `CategoryAttribute` holds full navigation properties to these unit types (`Unit? DefaultUnit`, `GroupUnit? AllowedGroupUnit`), creating an implicit dependency across what should be separate aggregate boundaries. If Units are part of the Catalog aggregate, then `GroupUnit` must be a child entity of an aggregate root (perhaps a new `UnitGroup` aggregate root or part of `Category`). If they are a separate aggregate, `CategoryAttribute` must only hold the `DefaultUnitId` GUID, not the entity reference. As written, this is a **tactical DDD violation**: entities with managed collections that lack an aggregate root marker, and cross-aggregate navigation properties that should be identity-only.

## Child Entity Encapsulation and Method Visibility

The design of child entities is a **clear strength** of this codebase. `ProductVariant`, `ProductImage`, `VariantImage`, `CategoryAttribute`, `AttributeOption`, and `AttributeValue` all use `internal` constructors and `internal` mutation methods. This means they can only be created or modified through their parent aggregate root, which is precisely what DDD prescribes. The aggregate roots expose public factory methods (`Create`) and public command methods (`AddVariant`, `AddImage`, `AddAttribute`, `Activate`, `Deactivate`). Collections are correctly exposed as `IReadOnlyCollection` via `AsReadOnly()`. The architecture tests reinforce this: `NotAggregateRootEntities_Should_NotHasPublicMethod` ensures no child entity leaks public methods.

**One gap**: `Product.RemoveVariant()` removes the variant from the `_variants` list but does not cascade-clean the variant's own images (`VariantImage`). Within an aggregate, referential integrity should be absolute — when a variant goes, its images must go too. This is a minor but real encapsulation leak.

## Domain Events: Placement, Naming, and Completeness

Events are placed in a dedicated `Events/` folder per aggregate, follow the `{Entity}{Action}DomainEvent` naming convention, and the architecture test enforces the `DomainEvent` suffix. All events are sealed records implementing `IDomainEvent` — correct.

**However, the Catalog domain suffers from significant event anemic-ness.** Only `ProductCreated`, `ProductUpdated`, `CategoryCreated`, and `CategoryUpdated` exist. Compare this to the Auth domain, which has `UserRegistered`, `UserDisabled`, `UserEnabled`, `UserLoggedIn`, `UserLoggedOut`, `RefreshTokenReused`, `UserProfileCreated`, `UserProfileUpdated` — each representing a distinct business occurrence. In Catalog, the following mutations raise **no domain event at all**:

- `Category.ChangeParent()`
- `Product.ChangeCategory()`
- `GroupUnit.Rename()`
- `GroupUnit.AddUnit()`
- `Unit.UpdateExchangeRate()`
- `CategoryAttribute.Update()`
- `AttributeOption.Update()`
- `ProductVariant.UpdateDetails()`
- `ProductVariant.Activate/Deactivate()`
- `AttributeValue.UpdateValue()`
- `ProductImage.Update()`
- `VariantImage.Update()`

Even worse, `Category.AddAttribute()` and `Product.AddVariant()` only trigger a generic `ProductUpdatedDomainEvent` or `CategoryUpdatedDomainEvent`, losing the semantic meaning that an attribute was added vs. any other update. When domain events are too generic, event handlers must re-query to understand *what* happened, defeating much of the value of event-driven design. The Auth domain is far more mature here.

## Value Objects: Design Quality and Validation

`Slug` and `Sku` are both `sealed record` types implementing `IValueObject` — immutable, self-validating, with regex constraints. This is solid.

**However, there is a critical backdoor in `Slug`**: the `From(string slug)` method calls the private constructor directly (`new Slug(slug)`) without any validation or sanitization. This bypasses all the sanitization logic in `Create()`, allowing creation of invalid slugs. The Auth value objects do not have this dangerous escape hatch.

Additionally, the Catalog value objects expose only a one-way implicit conversion (`implicit operator string` from the value object), whereas the Auth value objects consistently expose bidirectional implicit conversions. This inconsistency isn't harmful per se, but it breaks the expected pattern. The `Sku` value object throws a `DomainException` on validation failure while `Slug` uses `Guard` — another internal inconsistency in error signaling within the same aggregate.

## Anemic Domain Model Smells and Ubiquitous Language

The core entities (`Product`, `Category`, `ProductVariant`) are **not anemic** — they carry meaningful behavior (`AddVariant`, `AddAttribute`, `Activate`, `ChangeCategory`). However, `GroupUnit` and `Unit` are thin wrappers over data with trivial setters and no domain logic. Their naming is also suspect: "GroupUnit" and "Unit" are technical-internal terms, not business ubiquitous language. In a retail domain, the business would talk about "Weight (kg)" or "Length (cm)" — these are **measurement units**, not abstract "Units" and "GroupUnits." The folder name `Units/` is too generic. The `CategoryAttribute` entity stores `DefaultUnitId` and `AllowedGroupUnitId`, suggesting measurement units are a cross-cutting concern; the domain language should reflect that. Elsewhere, the naming is consistent: `CategoryName`, `Slug`, `IsActive`, `SortOrder`, `AttributeInputType`, `MultiSelect`, `Sku` — all are proper domain terms.

## Tactical DDD Violations Summary

1. **`GroupUnit` acts as an aggregate root without `IAggregateRoot`** — manages child entities, has a factory, but lacks the marker and raises no domain events.
2. **Cross-aggregate navigation properties** — `CategoryAttribute` holds direct entity references to `Unit` and `GroupUnit` instead of identity-only `Guid` references.
3. **Inconsistent error handling** — `Category.ChangeParent()` throws a plain `InvalidOperationException`, while `Sku.Create()` throws `DomainException`. Neither uses the `Result`/`Error` pattern that the rest of the application relies on. The Auth domain has the same dual-pattern problem (both `AuthErrors` static errors and `Exceptions.DomainException`), so this is a codebase-wide issue, but the Catalog domain exacerbates it.
4. **Missing domain events on most mutations** — only 4 of ~15 mutation points raise events, and the ones that do are semantically weak.
5. **No invariant enforcement in the domain layer** — nothing prevents deactivating a category with active products, changing a product's category to a non-existent one, or creating duplicate attribute names or SKUs. These invariants appear to be pushed to the application layer, weakening the domain model.
6. **`Slug.From()` bypasses validation** — a value-object integrity hole that should be removed or made private.
