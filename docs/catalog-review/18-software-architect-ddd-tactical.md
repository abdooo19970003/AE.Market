# Software Architecture Evaluation: DDD Tactical Patterns

## Executive Summary

The Catalog aggregate is a solid, well-structured implementation that follows DDD tactical patterns far better than most production code. The aggregate boundaries around **Product** (the primary aggregate) are well-chosen, value objects are immutable and properly validated, and domain events are raised inside the domain layer for most operations. However, there are several notable gaps: the **Product** entity has an **implicit aggregate relationship** via `ShippingDimensions` (mapped in EF but missing from the domain model), `ProductRelation` leaks encapsulation through a public mutator, and `GroupUnit` is arguably over-engineered as a separate aggregate root. The overall architecture is mature but contains some inconsistencies, particularly around domain event completeness (stock operations) and cross-aggregate concerns.

---

## PROS

### Aggregate Boundaries & References

- **Aggregates reference each other by ID, not by object reference (correct)**  
  `Product` references `Category` (via `Guid CategoryId`), `Brand` (via `Guid BrandId`), and `TaxCode` (via `Guid TaxCodeId`) using foreign key IDs only. This is the DDD-correct pattern for cross-aggregate references.  
  *File: `Domain/Aggregates/Catalog/Products/Product.cs:25,38,40`*

- **Child entities are accessed exclusively through aggregate root behavioral methods**  
  Variants, images, tags, relations, and attribute values can only be added/removed via `Product.AddVariant()`, `Product.AddImage()`, `Product.AddTag()`, etc. There is no way to reach into the aggregate and manipulate children directly from application code.  
  *File: `Domain/Aggregates/Catalog/Products/Product.cs:175-320`*

- **Consistency boundary protects invariants during activation**  
  `Product.Activate()` enforces that non-simple/digital products must have at least one variant before activation (line 159-160). `Product.RemoveVariant()` prevents removing the last variant while the product is active (line 186-187).  
  *File: `Domain/Aggregates/Catalog/Products/Product.cs:159,186`*

- **Category enforces tree consistency invariants**  
  `Category.ChangeParent()` prevents self-referencing and circular descendant relationships via `IsDescendantOf()`.  
  *File: `Domain/Aggregates/Catalog/Category.cs:110-125,127-137`*

### Value Objects

- **Immutable records with private constructors**  
  `Slug`, `Sku`, and `URL` are all `sealed record` types with `private` constructors and static factory methods. They are fully immutable, validate on construction, and implement the `IValueObject` marker interface.  
  *Files: `Domain/Aggregates/Catalog/ValueObjects/Slug.cs`, `Sku.cs`, `URL.cs`*

- **Meaningful validation in factory methods**  
  `Sku.Create()` validates regex pattern `^[A-Z0-9][A-Z0-9\-]{2,49}$`. `Slug.Create()` sanitizes and truncates. `URL.CreateAbsolute()` validates URI scheme.  
  *Files: `Domain/Aggregates/Catalog/ValueObjects/Sku.cs:23-27`, `Slug.cs:19-28`, `URL.cs:11-24`*

### Encapsulation & Invariants

- **Private setters on all entity properties**  
  Every property across all entities uses `private set` or `init`, preventing accidental external mutation.  
  *All entity files in `Domain/Aggregates/Catalog/`*

- **Collections exposed as `IReadOnlyCollection<T>`**  
  Every internal `List<T>` is exposed as `IReadOnlyCollection<T>` via `_list.AsReadOnly()`. External code cannot add/remove items.  
  *Examples: `Product.cs:43,46,49,52,55`, `Category.cs:25,28,31`*

- **`internal` methods on child entities**  
  `ProductVariant`, `ProductImage`, `Tag`, `Unit`, `CategoryAttribute`, `VariantImage`, `VariantAttributeValue` all use `internal static Create()` and `internal` mutators. This correctly restricts instantiation and mutation to within the aggregate boundary.  
  *Files: `ProductVariant.cs:45`, `ProductImage.cs:26`, `Unit.cs:33`, `CategoryAttribute.cs:47`*

- **Static factory methods as the only creation path**  
  Every aggregate root (`Product.Create()`, `Category.Create()`, `Brand.Create()`, `ProductTaxCode.Create()`, `GroupUnit.Create()`) uses a static factory method, not a public constructor.  
  *Files: `Product.cs:81`, `Category.cs:59`, `Brand.cs:48`, `ProductTaxCode.cs:27`, `GroupUnit.cs:24`*

### Domain Events

- **Events raised inside the domain, not the application layer**  
  All domain events are raised within entity behavioral methods (e.g., `Product.Activate()` raises `ProductActivatedDomainEvent`). The `CreateProductCommandHandler` never directly publishes events.  
  *Example: `Product.cs:162` vs `Application/.../CreateProductCommandHandler.cs`*

- **Rich context on most events**  
  Several events carry old/new values for meaningful subscribers. `VariantStockAdjustedDomainEvent` carries `OldQuantity, NewQuantity, Delta`. `ProductSlugChangedDomainEvent` carries `OldSlug, NewSlug`.  
  *Files: `VariantStockAdjustedDomainEvent.cs`, `ProductSlugChangedDomainEvent.cs`*

### Repository & Persistence

- **Repositories are aggregate-root-specific**  
  Although `IRepository<T>` is generic, it is always parameterized with an aggregate root type: `IRepository<Product>`, `IRepository<Brand>`, `IRepository<Category>`. No repository is created for child entities like `ProductVariant` or `ProductImage`.  
  *Files: `CreateProductCommandHandler.cs:11`, `CreateBrandCommandHandler.cs:11`, `CreateCategoryCommandHandler.cs:11`*

- **Specification pattern for querying**  
  Read queries use `ISpecification<T>` with eager loading, correctly separating query concerns from aggregate behavior. `ProductByIdSpec` conditionally includes children.  
  *Files: `Application/Features/Catalog/Specs/ProductByIdSpec.cs`, `ProductsByCategorySpec.cs`*

---

## GAPS

### Aggregate Boundaries (Medium)

- **`GroupUnit` is a questionable aggregate root**  
  `GroupUnit` (with child `Unit`) has no references from any other aggregate -- neither `Product`, `Category`, nor `Brand` reference it. It is purely reference data with unit conversion logic. Making it a separate aggregate root creates unnecessary transactional boundaries without justification. It would be better as a standalone entity managed through a dedicated service, or collapsed into the Catalog aggregate as a child of a larger `Measurement` context.  
  *File: `Domain/Aggregates/Catalog/Units/GroupUnit.cs:8`*

- **`Brand` and `ProductTaxCode` as aggregate roots are justified but borderline**  
  Both are referenced by ID from `Product`, which is correct. However, they have no behavioral invariants worth protecting (no business rules beyond CRUD). They are essentially reference data that could have been value objects or managed via a simpler pattern.  
  *Files: `Brand.cs:7`, `ProductTaxCode.cs:6`*

- **`ShippingDimensions` lives in `Inventory` aggregate but is mapped on `Product`**  
  `ShippingDimensions` is defined in `Domain/Aggregates/Inventory/` but the EF configuration for `Product` (Catalog aggregate) maps it as an owned entity (`builder.OwnsOne(x => x.ShippingDimensions, ...)`). Yet `Product.cs` does NOT have a `ShippingDimensions` property. This means either: (a) the property is missing from the domain model (a critical gap), or (b) the EF config references a non-existent property (a configuration error). Either way, this cross-aggregate dependency violates aggregate independence.  
  *Files: `Domain/Aggregates/Inventory/ShippingDimensions.cs`, `Infrastructure/.../Catalog/ProductConfiguration.cs:37`, `Domain/Aggregates/Catalog/Products/Product.cs` (no ShippingDimensions found)*

### Domain Events - Completeness (High)

- **`SetAllowBackOrder` does not raise a domain event**  
  `Product.SetAllowBackOrder()` updates `AllowBackOrder` and `BackOrderLimit` but only calls `UpdateLastModified()`. No domain event is published. This is a meaningful business state change that subscribers (inventory, notifications) may need to react to.  
  *File: `Domain/Aggregates/Catalog/Products/Product.cs:267-272`*

- **`ProductVariant.ReserveStock` and `ReleaseStock` do not raise domain events**  
  These methods modify `ReservedQuantity` (a critical inventory concern) without publishing any domain event. `VariantStockAdjustedDomainEvent` is created but never raised for reserve/release operations.  
  *File: `Domain/Aggregates/Catalog/Products/Variants/ProductVariant.cs:144-156`*

- **`AttributeGroup.Rename` does not raise a domain event**  
  Renaming an attribute group is a meaningful change to the attribute taxonomy that should be communicated.  
  *File: `Domain/Aggregates/Catalog/Attributes/AttributeGroup.cs:32-38`*

### Domain Events - Naming Consistency (Low)

- **Inconsistent event file naming**  
  Some event files are named `{Name}Event.cs` (without `DomainEvent` suffix) while containing a class named `{Name}DomainEvent`. Examples: `ProductCreatedEvent.cs` contains `ProductCreatedDomainEvent`, `CategoryDeletedEvent.cs` contains `CategoryDeletedDomainEvent`. Others like `ProductActivatedDomainEvent.cs` are consistent between filename and class name. This makes discovery harder.  
  *Files: `ProductCreatedEvent.cs` vs `ProductActivatedDomainEvent.cs`, `CategoryDeletedEvent.cs` vs `CategoryActivatedDomainEvent.cs`, `BrandCreatedEvent.cs` vs `BrandActivatedDomainEvent.cs`, `GroupUnitCreatedEvent.cs` vs `GroupUnitUnitAddedDomainEvent.cs`*  
  *Note: Auth events have the same issue (e.g., `UserRegisteredEvent.cs` contains `UserRegisteredDomainEvent`), so it is a project-wide convention inconsistency.*

---

## ISSUES

### CRITICAL

- **`ProductRelation.UpdateSortOrder()` is `public` instead of `internal`**  
  This is an encapsulation breach. `ProductRelation` is a child entity of the `Product` aggregate, yet its `UpdateSortOrder` method is `public`, meaning any external code can modify it without going through the `Product` aggregate root. This bypasses the consistency boundary and could lead to invariant violations. All other child entity methods (e.g., `ProductImage.Update()`, `Tag.Create()`) correctly use `internal`.  
  *File: `Domain/Aggregates/Catalog/Products/ProductRelation.cs:38-41`*

- **`Category` uses object references for parent/child traversal in business logic**  
  `Category.IsDescendantOf()` and `CollectAncestorAttributes()` recursively traverse the `Category.Parent` and `_subCategories` object references. This requires the entire ancestor or descendant chain to be loaded into memory, which is impractical for deep category hierarchies and creates a heavy implicit loading requirement. The `CollectAncestorAttributes` method traverses `category.Parent` (object reference) while the data is persisted via `ParentId` (foreign key). These methods should use repository queries or ID-based traversal instead.  
  *Files: `Domain/Aggregates/Catalog/Category.cs:127-137,146-156,160-169`*

### HIGH

- **`Product` entity is missing `ShippingDimensions` property despite EF mapping**  
  The `ProductConfiguration.cs` maps `ShippingDimensions` as an owned entity via `builder.OwnsOne(x => x.ShippingDimensions, ...)`, but the `Product` domain class has no such property. This is either a domain model gap (product dimensions are a core catalog concern that is not modeled) or a stale EF configuration.  
  *File: `Infrastructure/.../Catalog/ProductConfiguration.cs:37`; confirmed absent in `Domain/Aggregates/Catalog/Products/Product.cs`*

- **`ProductRelation.UpdateSortOrder` is public (see CRITICAL above - re-listed for severity)**  
  Same as the critical issue above. This represents both a CRITICAL encapsulation breach and a HIGH issue because it undermines the aggregate boundary for one of the product's child entities.

### MEDIUM

- **Domain events for initial brand/tax code assignment create noise**  
  In `CreateProductCommandHandler`, `UpdateBrand()` and `UpdateTaxCode()` are called immediately after `Product.Create()`. Since the brand and tax code are being set for the first time, raising `ProductBrandChangedDomainEvent` and `ProductTaxCodeChangedDomainEvent` at this point is semantically incorrect (nothing "changed" -- it was just assigned). The factory method or a dedicated `SetBrand(guid)` should be used instead, one that does not raise a "changed" event for initial assignment.  
  *File: `Application/.../CreateProductCommandHandler.cs:28-29`*

- **Some domain events carry only the entity ID without sufficient context**  
  `ProductDeletedDomainEvent` only carries `ProductId`, `BrandDeletedDomainEvent` only carries `BrandId`, and `ProductTaxCodeUpdatedDomainEvent` only carries `TaxCodeId`. While this may be sufficient for some subscribers, richer events (like `VariantStockAdjustedDomainEvent` which includes old/new values) are a better pattern for decoupled handlers that may need the data without performing additional queries.  
  *Files: `ProductDeletedEvent.cs`, `BrandDeletedEvent.cs`, `ProductTaxCodeUpdatedEvent.cs`*

- **`Product.Create` does not validate that `categoryId` or other FK references are non-empty**  
  While validation of existence is an application-layer concern, a basic `Guard.AgainstDefault(categoryId)` or similar check could prevent accidental `Guid.Empty` values from being persisted.  
  *File: `Domain/Aggregates/Catalog/Products/Product.cs:81-94`*

### LOW

- **`Sku` and `Slug` value objects use `implicit operator string`**  
  `public static implicit operator string(Slug slug) => slug.Value;` and similarly for `Sku`. While convenient, this can lead to accidental type erasure where a strongly-typed `Slug` is silently converted to `string` and loses its semantic meaning.  
  *Files: `Slug.cs:40`, `Sku.cs:34`*

- **`ProductVariant` exposes `RowVersion` as a public read-only property**  
  `public byte[] RowVersion { get; private set; }` is a persistence concern (optimistic concurrency) leaking into the domain model. While it has a private setter and is not mutable externally, its presence in the domain entity couples it to the persistence mechanism.  
  *File: `Domain/.../Variants/ProductVariant.cs:21`*

- **`AttributeGroup.Delete()` is `public` while most methods are `internal`**  
  All instance methods on `AttributeGroup` are `internal` except `Delete()` (which is `public override`). This is a minor inconsistency.  
  *File: `Domain/.../Attributes/AttributeGroup.cs:51`*

- **`Product` computed properties (`SalePrice`, `StockQuantity`) iterate all variants each time**  
  `SalePrice` and `StockQuantity` on `Product` iterate `_variants` with LINQ. For products with many variants, these could be expensive if called repeatedly. Minor performance concern.  
  *File: `Domain/.../Products/Product.cs:57-58`*

---

## Recommendations (Top 5)

1. **Fix the `ShippingDimensions` gap (CRITICAL)**  
   Either add `ShippingDimensions` as a value object property to `Product` (recommended, as dimensions are a core catalog concern), or remove the EF mapping from `ProductConfiguration.cs`. If added, ensure the value object stays in the Catalog context rather than referencing the Inventory aggregate.

2. **Change `ProductRelation.UpdateSortOrder()` from `public` to `internal` (CRITICAL)**  
   This is a one-line fix that restores the aggregate boundary. All child entity mutation must go through the `Product` aggregate root.

3. **Add missing domain events (HIGH)**  
   Raise events for `Product.SetAllowBackOrder()`, `ProductVariant.ReserveStock()`, and `ProductVariant.ReleaseStock()`. These are semantically meaningful state changes that other parts of the system (notifications, inventory tracking, analytics) need to react to.

4. **Replace object-reference tree traversal with ID-based approach in Category (HIGH)**  
   `Category.IsDescendantOf()` and `CollectAncestorAttributes()` should use a repository/specification query to determine ancestry/descendant relationships rather than traversing in-memory object graphs. This avoids loading entire category trees and works correctly with lazy/partial loading scenarios.

5. **Normalize domain event file naming (LOW) and review initial-assignment events (MEDIUM)**  
   Align all event file names to include the `DomainEvent` suffix (e.g., rename `ProductCreatedEvent.cs` to `ProductCreatedDomainEvent.cs` to match the class name). For "initial assignment" events (`ProductBrandChangedDomainEvent` during creation), introduce a `SetBrand(Guid brandId)` method on `Product` that does not raise a "changed" event, reserving `UpdateBrand()` for actual changes.
