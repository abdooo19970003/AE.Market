# DDD Domain Expert — Required Attribute Enforcement Decision

## Analysis

### Current State

1. **`Product.Create()`** and **`ProductVariant.Create()`** accept zero attribute values — products/variants are born with an empty `_attributeValues` list. No invariant check at creation.

2. **`Product.SetAttributeValue()`** (public) and **`ProductVariant.SetAttributeValue()`** (internal) are the only way to add attribute values, but they are purely additive — they do not check whether the attribute is required by the product's category.

3. **`Product.HasAllRequiredAttributes(requiredIds)`** and **`GetMissingRequiredAttributeIds(requiredIds)`** exist, are unit-tested, but are **never called from production code** — the error codes `ProductMissingRequiredAttributes` and `VariantMissingRequiredAttributes` in `CatalogErrors.cs` are also **dead code**.

4. **`Product.Activate()`** validates other invariants (variants exist for configurable, bundle items for bundle, super-attributes for configurable) but does **NOT** check `HasAllRequiredAttributes()`.

5. **`ProductVariant`** has **no** `HasAllRequiredAttributes()` equivalent at all.

6. **No `ActivateProductCommand` / `ActivateVariantCommand`** exists in the application layer. `Activate()` and `Deactivate()` are only called from tests.

7. **Cross-aggregate boundary**: Product holds only a `CategoryId` (weak reference). To know which attributes are required, you must load the `Category` aggregate and call `GetEffectiveAttributes()`. This is a cross-aggregate query that Product cannot perform internally.

8. **`Category.GetEffectiveAttributes()`** walks the category's parent chain and collects all `CategoryAttribute` entries including their `IsRequired` flag.

---

## Recommendation

### 1. Enforce at activation time, NOT creation time

The invariant "product/variant must have all required attribute values" should be enforced at **activation time**. Reasons:

- **Respects aggregate boundaries**: Product does not own the Category aggregate. It has only a `CategoryId` weak reference. The set of required attribute IDs comes from the Category (via `GetEffectiveAttributes()`), which is a separate aggregate root. Loading the Category to validate at creation would either require passing it into `Product.Create()` (leaking cross-aggregate concerns into the domain factory) or doing an out-of-process fetch inside the domain (infeasible). Activation is naturally handled by the application layer, which can load both aggregates.

- **Matches existing architectural pattern**: `Product.Activate()` already serves as the invariant gate — it checks for at least one variant (configurable products), at least one super-attribute (configurable), and at least one bundle item (bundle products). Adding a required-attribute check is consistent with this existing enforcement point.

- **Supports realistic UX flows**: Products are commonly created as skeletons first (name, SKU, category, type) and then enriched with attribute values in subsequent steps before going live. Requiring all values at creation would force either (a) a monolithic create-with-attributes command, or (b) a multi-step create where the first step is artificially constrained.

### 2. Add `HasAllRequiredAttributes()` to `ProductVariant`

`ProductVariant` has no `HasAllRequiredAttributes()` or `GetMissingRequiredAttributeIds()` method. It needs one, analogous to `Product`'s, so the variant activation path can also be validated. The required attributes for a variant are the **variant-defining (super) attributes** of its parent configurable product, plus any category-level required attributes.

### 3. Wire the check into `Product.Activate()` by accepting required IDs

Change `Product.Activate()` to optionally accept the required attribute IDs and enforce the invariant internally.

The application-layer activation command handler would load the Category, compute `GetEffectiveAttributes().Where(a => a.Attribute.IsRequired).Select(a => a.Attribute.Id)`, pass those IDs to `product.Activate(requiredIds)`, and then save.

This keeps the invariant logic in the domain (Product enforces it itself) while acknowledging that the data source (the list of required IDs) comes from outside the aggregate boundary.

### 4. Do NOT change `Create` / `AddVariant` signatures

`Product.Create()` and `Product.AddVariant()` should **not** accept attribute values as parameters. Keeping them simple avoids bloating the factory/command method with cross-aggregate validation concerns. The existing `SetAttributeValue()` methods (public on Product, internal on ProductVariant) remain the correct mechanism for adding attribute values after creation, called from the application layer's `SetProductAttributeValueCommand`.

### 5. Aggregate boundary is correct

Product is the correct aggregate root. It owns its variants, images, tags, attribute values, relations, and bundle items as child entities. Category is correctly a separate aggregate root with only a `CategoryId` weak reference from Product. The category—product relationship is a reference, not a composition. Cross-aggregate validation (matching product attribute values against category-defined required attributes) is a legitimate application-layer concern, not a signal that the boundary is wrong.
