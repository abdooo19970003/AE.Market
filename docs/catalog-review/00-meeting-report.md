# 📋 Catalog Domain Review — Team Meeting Report

**Attendees:**

| Role | Name | Expertise |
|---|---|---|
| 🛒 Domain Expert — Selling & Merchandising | Aisha | Decades in retail selling, e-commerce merchandising, pricing strategy |
| 📢 Domain Expert — Marketing & SEO | Marcus | Digital marketing, SEO, product content strategy |
| 🏷️ Domain Expert — Product Classification | Priya | Taxonomy design, catalog management, faceted search |
| 🏗️ Software Architect — DDD | Elena | Domain-Driven Design purity, aggregate boundaries, tactical patterns |
| ⚙️ Software Architect — Clean Architecture | Omar | CQRS, .NET backend systems, MediatR pipelines, EF Core |
| 👔 Project Manager | James | Delivery management, risk assessment, sprint planning |

---

## 1. Opening: State of the Catalog

**James (PM):** "We have a solid Domain layer — 85% complete per my assessment — but zero Infrastructure, zero Application, zero API, zero tests. The team needs to review what's there, flag gaps, and agree on a sprint plan."

---

## 2. Consensus: What Works Well

All six agreed on these strengths:

- **Aggregate boundaries:** Product and Category as separate aggregate roots with weak reference (Guid) is correct. "Best-practice DDD," said Elena.
- **Child encapsulation:** Internal constructors/methods on ProductVariant, ProductImage, VariantImage, AttributeValue, etc. — enforced by architecture tests. "Hard to overstate how rare and how valuable this is in a greenfield project," said Elena.
- **Value objects:** Slug and Sku are well-validated, immutable records. "Solid foundation," said Omar.
- **Attribute system:** CategoryAttribute with IsFilterable, AttributeInputType, AttributeOption, and Typed Value storage. "Exactly right for faceted search," said Priya.
- **Unit of measure:** GroupUnit/Unit with base unit exchange rates. "Sophisticated and flexible," said Aisha.
- **Domain events structure:** Events folder, sealed records, IDomainEvent pattern.
- **Code conventions:** File-scoped namespaces, primary constructors, private setters — consistent with Auth.

---

## 3. Critical Disagreements & Flagged Issues

### 🔥 Hot Debate: GroupUnit/Unit — Aggregate Root Violation?

**Elena (DDD):** "GroupUnit manages child entities (Unit) and has factory methods but lacks `IAggregateRoot`. Also, CategoryAttribute holds direct entity references to Unit/GroupUnit instead of GUID-only references. This is a tactical DDD violation."

**Omar (Clean Arch):** "Agree in principle. But for pragmatic reasons, if Units are an internal classification detail fully owned by Catalog, we could merge them into Category as child entities, or promote GroupUnit to a small aggregate root. Pick one and move on."

**Consensus:** GroupUnit needs either `IAggregateRoot` marker or must become a child of Category. Cross-entity navigation properties must be replaced with ID-only references. **Action item for next sprint.**

### 🔥 Debate: Do We Need ProductAttributeValue?

**Priya (Classification):** "Yes. Brand, Manufacturer, Warranty, Material — these are product-level, not variant-level. The current model forces them into a dummy variant."

**Aisha (Selling):** "Strong agree. Every real e-commerce platform I've worked on has product-level attributes separate from variant attributes. Without this, you can't filter by Brand at the product level."

**Omar (Clean Arch):** "This doubles the attribute work — new entity, new EF config, new commands. But it's the right call. Defer to Sprint B."

**James (PM):** "Noted. Deferred — needs product owner sign-off."

### 🔥 Debate: Is Product.Sku Missing?

**James (PM):** "BACKEND_PLAN has `products.sku` as unique. Current Product entity has no Sku — only ProductVariant does."

**Aisha (Selling):** "In retail, the parent product often carries a SKU for inventory aggregation. Variant SKUs are for fulfillment. Both should exist."

**Elena & Omar:** "Add it to Product as a Sku value object. Low effort, high impact."

**Consensus:** Add `Product.Sku` (Sku value object). **Agreed — do in current sprint.**

### 🔥 Debate: Slug.From() Bypasses Validation

**Elena (DDD):** "`Slug.From(string slug)` calls the private constructor directly, bypassing all sanitization. This is a value-object integrity hole."

**Omar:** "Remove it or make it private. The only public path should be `Slug.Create()` or implicit conversion."

**Consensus:** Remove or privatize `Slug.From()`. **Agreed — quick fix.**

### 🔥 Debate: Exception vs. Result Pattern

**Elena & Omar:** "`Category.ChangeParent()` throws `DomainException`. The project convention is `Result<T>` return. This should be consistent."

**James (PM):** "Noted. Low priority but should align. Defer to Sprint C."

---

## 4. Domain Expert Gaps (Priority-Ordered)

From the three domain experts, the top missing features:

| # | Gap | Domain Expert(s) | Impact |
|---|---|---|---|
| 1 | **Pricing (ListPrice, SalePrice, CostPrice)** | Aisha (Selling) | **CRITICAL** — catalog is not sellable without prices |
| 2 | **Inventory (StockQuantity, TrackInventory, AllowBackorder)** | Aisha | **CRITICAL** — cannot operate a store without stock |
| 3 | **ProductType enum (Simple, Configurable, Bundle, Digital)** | Aisha, Priya | **HIGH** — current model forces variants on all products |
| 4 | **Brand entity** | Priya, Marcus, Aisha | **HIGH** — top-3 filter on every e-commerce site |
| 5 | **Category SEO (MetaTitle, MetaDescription, Robots)** | Marcus | **HIGH** — categories are highest-traffic landing pages |
| 6 | **Canonical URL field** | Marcus | **HIGH** — duplicate content kills SEO for variants/filtered views |
| 7 | **Short vs. Long Description** | Marcus, Aisha | **MEDIUM** — listing cards vs. product detail pages |
| 8 | **Product Tags (many-to-many)** | Marcus | **MEDIUM** — flexible grouping for campaigns |
| 9 | **Badges/Labels (New, Sale, Best Seller)** | Marcus | **MEDIUM** — visual promotional indicators |
| 10 | **Shipping dimensions (Weight, LxWxH)** | Aisha, Priya | **MEDIUM** — carrier rate calculation |
| 11 | **Tax category** | Aisha | **MEDIUM** — legal/VAT compliance |
| 12 | **Product relationships (Related, Cross-sell, Up-sell)** | Aisha, Marcus | **MEDIUM** — merchandising & AOV drivers |

**James (PM):** "Items 1, 2, 3, and 4 are pre-MVP essentials. Items 5–12 are post-MVP. But Pricing and Inventory are **not** in the current BACKEND_PLAN scope for Catalog — they're assigned to future Pricing/Inventory aggregates. The domain team needs to decide: embed prices in Catalog (simpler but less clean), or accept Catalog is not sellable until the Pricing aggregate exists."

**Aisha (Selling):** "Then the catalog is a brochure, not a selling system. At minimum, add ListPrice to ProductVariant. That's one field — not a whole aggregate. You can move it later."

**Consensus:** Add `ListPrice` (decimal) to ProductVariant as a pragmatic MVP compromise. Full pricing aggregate is post-MVP.

---

## 5. DDD & Clean Architecture Technical Debt

From Elena and Omar:

| Issue | Severity | Fix |
|---|---|---|
| GroupUnit acts as aggregate root without marker | Medium | Add `IAggregateRoot` or restructure |
| Cross-aggregate navigation (CategoryAttribute → Unit) | Medium | Replace with ID-only refs |
| Missing domain events on most mutations | Medium | Add events: VariantAdded, CategoryMoved, AttributeAdded, etc. |
| No variant cascade cleanup on `Product.RemoveVariant()` | Low | Remove child VariantImages in same method |
| Product.Slug & ProductVariant.Sku are raw strings | Low-Medium | Replace with Slug/Sku value objects |
| No invariant enforcement at domain layer (unique SKU, etc.) | Low | Push to application layer for now |
| CacheKeys empty (Application) | High | Populate before any caching works |
| 4 Event handlers return Task.CompletedTask | High | Wire up cache eviction + search indexing |

---

## 6. Project Manager's Verdict & Action Plan

**James (PM):** "We have a strong foundation but a mountain of work. Here's the plan."

### 🚀 Sprint A (Weeks 1–2) — Infrastructure Foundation + Categories Vertical Slice

1. ✅ Fix `Slug.From()` validation bypass
2. ✅ Add `Product.Sku` (Sku value object) to Product entity
3. ✅ Add `ListPrice` to ProductVariant
4. ✅ Fix GroupUnit aggregate root / navigation properties
5. ✅ Create EF Configurations for all Catalog entities (10 files)
6. ✅ Register DbSets in AppDbContext + generate migration
7. ✅ Populate CacheKeys
8. ✅ Implement Categories end-to-end (CreateCategory, GetCategoryTree, etc.)
9. ✅ CategoriesController
10. ✅ Domain tests for Category

### 🚀 Sprint B (Weeks 3–4) — Products + Variants

1. ✅ Add ProductAttributeValue entity (product-level EAV)
2. ✅ Add domain events for all mutation points
3. ✅ Implement Products Application (CRUD + listing)
4. ✅ Implement Variants Application (CRUD)
5. ✅ Wire event handlers for cache eviction
6. ✅ ProductsController
7. ✅ Domain tests for Product, ProductVariant, ProductImage
8. ✅ Architecture tests for Catalog

### 🚀 Sprint C (Week 5–6) — Polish + Edge Cases

1. ✅ Add Brand entity (if prioritized)
2. ✅ Fix `ChangeParent()` exception → Result pattern
3. ✅ Seed data for Catalog
4. ✅ Integration tests
5. ✅ Caching strategy (ICachedQuery on Catalog queries)
6. ✅ SEO fields on Category (MetaTitle, etc.)

---

## 7. Key Decisions Made

| Decision | Outcome |
|---|---|
| Product.Sku? | ✅ Add to Product entity this sprint |
| Slug.From() validation hole? | ✅ Remove/privatize, quick fix |
| ProductAttributeValue? | ⏳ Deferred to Sprint B |
| Price on ProductVariant? | ✅ Add `ListPrice` as pragmatic MVP field |
| GroupUnit aggregate root? | ✅ Fix in Sprint A |
| Exception vs Result pattern? | ⏳ Deferred to Sprint C |
| Brand entity? | ⏳ Post-MVP / Sprint C |
| Category SEO fields? | ⏳ Deferred to Sprint C |
| Inventory fields? | ⏳ Blocked on Pricing/Inventory aggregate |
| ProductType enum? | ⏳ Needs product owner discussion |

---

**James (PM):** "Good meeting, team. The Domain is solid — no structural rewrites needed. The path is clear: build the Infrastructure bridge, then ship Categories as the first vertical slice. Elena and Omar will pair on the EF configs. Aisha, Marcus, Priya — please review the final Domain for any last gaps before Sprint A starts. Let's ship."
