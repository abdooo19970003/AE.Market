# Data Integrity & Database Expert — Required Attribute Enforcement Decision

## Analysis

### 1. Current State: No enforcement exists at any layer

- **Domain layer**: `Product.Create()` and `ProductVariant.Create()` do not accept attribute values. `Product.SetAttributeValue()` and `ProductVariant.SetAttributeValue()` exist, but neither checks `CategoryAttribute.IsRequired`. The only domain validation is `AttributeInputTypeValidator`, which checks *type correctness* (e.g., Text input must have a `ValueText`), not *required-ness*.

- **Application layer**: `CreateProductCommandHandler`, `AddProductVariantCommandHandler`, and `SetProductAttributeValueCommandHandler` all load product data but **never load the category's `CategoryAttribute` records to verify required attributes are satisfied**. No FluentValidation rule checks this either.

- **Database layer**: `product_attribute_values` has **no check constraints, no unique constraints, no partial indexes** — zero database-level safeguards.

- **Migration state**: The catalog tables (`products`, `product_variants`, `product_attribute_values`, etc.) are defined in EF configurations and `DbSet` properties, but **no migration has been generated yet** (only auth + outbox migrations exist in the project). This means the schema is still in a greenfield state.

### 2. Key structural observations about `product_attribute_values`

- **`ProductId` and `VariantId` are both nullable**, but there is no constraint ensuring exactly one is set. You could insert a row with both null, or both set.
- **No unique constraint on `(ProductId, AttributeId)`**: the same attribute can be assigned to a product multiple times (which would be data corruption).
- **No unique constraint on `(VariantId, AttributeId)`**: same problem for variants.

### 3. Can required attributes be enforced via check constraints?

**Not directly.** A PostgreSQL `CHECK` constraint cannot reference rows in another table (`category_attributes`). Since "required" is a dynamic property on `CategoryAttribute`, the database has no way to know at insert time which attributes are required for a given product's category without a join, which CHECK constraints cannot do.

A **trigger** (`BEFORE INSERT/UPDATE/DELETE` on `product_attribute_values`) could query `category_attributes` to verify that all `IsRequired = true` attributes have a value. However, triggers are opaque, hard to debug, complicate deployment, and become a performance concern under bulk operations.

### 4. What IS appropriate at the database level?

Three concrete safeguards are appropriate and cheap:

| Safeguard | Mechanism | Why |
|---|---|---|
| **Prevent duplicate attribute assignments** | Partial unique index `(ProductId, AttributeId) WHERE ProductId IS NOT NULL` and `(VariantId, AttributeId) WHERE VariantId IS NOT NULL` | Enforces the invariant that a product/variant has at most one value per attribute |
| **Ensure exactly one owner** | `CHECK ( (ProductId IS NOT NULL AND VariantId IS NULL) OR (ProductId IS NULL AND VariantId IS NOT NULL) )` | A value must belong to either a product or a variant, never both, never neither |
| **Prevent orphaned values by FK** | Foreign keys from `ProductId` -> `products(Id)` and `VariantId` -> `product_variants(Id)` with `ON DELETE CASCADE` | Already mapped from the principal side, but worth verifying the FK is generated in migrations |

These are static structural rules — perfect for database enforcement because they never change.

---

## Recommendation (5 bullet points)

1. **Add database-level structural constraints now, before the catalog migration is generated** — This is the most opportune time since no catalog migration exists yet. Add to `ProductAttributeValueConfiguration.cs`:
   - A CHECK constraint ensuring exactly one of `ProductId`/`VariantId` is non-null.
   - Two partial unique indexes preventing duplicate `(ProductId, AttributeId)` and `(VariantId, AttributeId)`.
   - These are cheap, static invariants that safeguard data integrity regardless of application bugs.

2. **Enforce required attributes at the application/domain level, not with database triggers** — Required-ness is a dynamic business rule defined on `CategoryAttribute` (cross-aggregate). Add validation in `CreateProductCommandHandler` and `AddProductVariantCommandHandler` that loads the category's `CategoryAttribute` records filtered by `IsRequired == true` and rejects the command if any required attribute is missing a value. This belongs in the handler (or a domain factory method), not in a trigger.

3. **Add validation to `SetProductAttributeValueCommandValidator`** — When setting a single attribute value, the validator should verify the attribute is valid for the product's category. For deletion of attribute values, the handler should refuse to remove the last value of a required attribute.

4. **Backfill / migration path** — Since no catalog data exists yet, there is no backfill needed. Going forward, add a `DatabaseSeeder` or idempotent startup check that validates all existing products have their required attributes. If this check runs in dev (similar to the existing auth seed), it prevents drift from accumulating.

5. **Rejecting enforcement solely at the database level is the right call for required attributes** — Business rules that dynamically depend on `CategoryAttribute.IsRequired` cross table boundaries and are subject to frequent change. Check constraints cannot express this, and triggers add maintenance burden. Application-layer enforcement (FluentValidation + domain methods) is the correct primary mechanism, with the static DB constraints above as a safety net.
