# Business / Product Owner — Required Attribute Enforcement Decision

## Analysis and Trade-Offs

### 1. Business Value of Allowing Draft Products Without Required Attributes

**Multi-stage product onboarding** — In real-world commerce, product data often comes from different sources at different times. A merchandiser may create a product shell (name, SKU, category, type) immediately after a supplier contract, but technical specs (weight, dimensions, certifications) arrive days later via a separate datasheet. Allowing draft creation without attributes supports this natural staggered workflow.

**Bulk import / ETL pipelines** — Bulk feeds (e.g., ERP syncs, marketplace integrations) rarely fit into a single atomic request. A typical pattern is: (1) create product records, (2) set attribute values, (3) activate once complete. Requiring all attributes at creation forces import logic into a complex multi-step orchestration or a single monolithic payload, increasing integration cost.

**Low-friction API contracts** — External integrators (dropshippers, PIM systems) can post a minimal product and enrich it later. Tight creation-time validation raises the bar for every integration and increases the probability of 400 errors during onboarding.

**Category attribute volatility** — Category managers may add new required attributes or mark existing ones as required. If all draft products must instantly satisfy the new requirements, many otherwise-valid drafts become orphaned. Lax creation insulates the product catalog from category schema changes.

### 2. Risk of Requiring All Attributes at Creation

**Increased abandonment rate** — A merchant who wants to list a simple physical product but doesn't know the exact weight yet is blocked from even creating the draft. This pushes users toward other systems or spreadsheets.

**Brittle API contract** — The `POST /api/products` endpoint must now accept a complex attributes payload, and the handler must load the category, compute effective required attributes, and validate. This couples the creation endpoint to the entire attribute infrastructure.

**False precision** — Some attribute values are genuinely unknown at creation time (e.g., future stock levels, dynamic dimensions from a third-party warehouse). Forcing a value encourages garbage data (defaults like "0" or "N/A") that pollutes the catalog.

**Category change propagation** — Adding a required attribute to a category retroactively breaks all future creation calls for that category until the API consumer updates their payload. Every category schema change becomes a breaking API change.

### 3. Impact on Existing Integrations

**Zero impact today** — The current `CreateProductCommandHandler` does not check attributes at all and does not call `HasAllRequiredAttributes()`. Existing integrators POST only name, slug, SKU, categoryId, type, and optional metadata. Enforcing attributes at creation would be a breaking change for every integration.

**If enforcement is added** — All external systems that create products would need to be updated to (a) fetch the category attributes, (b) populate required ones, or (c) call a new draft endpoint. This is a non-trivial migration for any connected system (ERP, PIM, marketplace connectors).

### 4. Is the Current `IsActive` Boolean Sufficient?

**No.** The current model has a significant gap:

- **Products default to `IsActive = true`** (line 19 of `Product.cs`). A freshly created product with zero attributes, zero variants, and no pricing is immediately "active" — meaning it could be considered purchasable (if it has at least one variant). This is inconsistent: the intent of the design is clearly that `Activate()` should be a guard gate, yet new products skip it entirely.

- **No explicit "draft" lifecycle state.** There is no `IsDraft` property, no status enum, and no workflow transitions. A product is either "active" or "inactive", and most consumers will treat inactive as deleted/hidden. There is no first-class concept of "work in progress."

- **`HasAllRequiredAttributes()` is defined and tested but never called.** The `ProductMissingRequiredAttributes` error exists but is never thrown. The domain's own infrastructure for enforcing completeness exists but is disconnected from the activation flow. This suggests the original intent was to validate on `Activate()`, but the wiring was never completed.

**Recommendation on state model:** Introduce a `ProductStatus` enum (e.g., `Draft`, `PendingActivation`, `Active`, `Suspended`) instead of a bare `IsActive` flag. This gives you a natural home for draft products and makes attribute validation a prerequisite for the `Draft -> Active` transition, not for creation.

### 5. Different Product Types and Attribute Requirements

| Product Type | Product-Level Attributes | Variant-Level Attributes | Key Activation Guard |
|---|---|---|---|
| **Simple** | Category-required attributes | N/A | At least 1 variant |
| **Digital** | Category-required attributes | N/A | At least 1 variant |
| **Configurable** | Category-required attributes + super (variant-definer) attributes | Variant-specific attribute values (Color, Size, etc.) | At least 1 variant + at least 1 super attribute |
| **Bundle** | Category-required attributes | N/A (uses `BundleItem` references) | At least 1 bundle item |

**Key insight:** Required category-level attributes apply identically across all product types. But for **Configurable** products, the *super attributes* (variant-definers) are a separate, stricter requirement — they must exist before activation (already enforced in `Activate()` via `ProductMissingSuperAttributes`). The gap is that category-level required attributes are never validated for any product type.

### 6. Bulk Import Scenarios

No bulk import infrastructure exists in the codebase (`grep` returns zero matches for "bulk", "import", "csv", "excel"). If/when bulk import is built, the current lax model is ideal: import creates drafts in a single pass, attributes are populated in a second pass (or via the same upload with a different sheet), and then a batch activation job calls `Activate()` which checks completeness. Enforcing attributes at creation would force all import logic into a single highly-coupled pipeline.

---

## Recommendation (4 bullet points)

1. **Do NOT enforce required attributes at creation time.** Keep product creation lightweight — accept only core identity fields (name, slug, SKU, category, type). This preserves multi-stage onboarding, protects integrations from breaking changes, and avoids coupling the creation endpoint to category schema. Create is not the right enforcement boundary.

2. **Wire `HasAllRequiredAttributes()` into the `Activate()` method.** The domain already has the method and error; they are simply never called. Fix this gap so that the `Activate()` transition (from inactive to active) enforces completeness against the category's effective required attributes. This aligns enforcement with the semantic boundary of "making the product available for sale." Same for `ProductVariant.Activate()` — wire in `VariantMissingRequiredAttributes`.

3. **Introduce a proper `ProductStatus` enum (Draft / PendingActivation / Active / Suspended) to replace the bare `IsActive` flag.** New products should default to `Draft`, not `Active`. This provides an explicit lifecycle, a home for incomplete products, and a clear semantic for the `Draft -> Active` transition that enforces attribute completeness. It also prevents the current bug where a freshly created product with no variants is technically "purchasable."

4. **Build the bulk import pipeline assuming multi-stage enrichment.** Design the import flow as: Stage 1 = create drafts, Stage 2 = set attribute values + variant data, Stage 3 = batch activation with completeness validation. Requiring attributes at creation would make this multi-stage approach impossible. The current lax model is well-suited to this design.
