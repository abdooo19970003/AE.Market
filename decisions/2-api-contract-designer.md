# API Contract Designer — Required Attribute Enforcement Decision

## Current State

| Aspect | Details |
|--------|---------|
| **`CreateProductCommand`** | No attribute fields — product created with only name, slug, sku, categoryId, brandId, etc. |
| **`AddProductVariantCommand`** | No attribute fields — variant created with only name and sku. |
| **Existing attribute endpoint** | `POST /api/products/{productId}/attributes` — sets **one** attribute at a time via `SetProductAttributeValueCommand`. No batch endpoint exists. |
| **Domain model** | `Product.SetAttributeValue(...)` and `ProductVariant.SetAttributeValue(...)` already exist. `CategoryAttributeDto.HasRequired` and `RequiredAttributeDto.IsRequired` track mandatory attributes. |
| **Consistency gap** | Products and variants can be created in an inconsistent state (missing required attributes). |

---

## Recommendation

### 1. Add an optional `List<AttributeValueDto>` to both `CreateProductCommand` and `AddProductVariantCommand`

Both commands should accept an **optional** list of attribute values so clients can create a fully-formed resource in one round trip when they choose to. The handler already has the product/variant entity in scope — `SetAttributeValue()` can be called directly after creation, avoiding a second DB lookup.

```csharp
public sealed record CreateProductCommand(
    // ... existing fields ...
    List<CreateAttributeValueDto>? AttributeValues = null   // NEW
) : ICommand<ProductDto>;
```

A new `CreateAttributeValueDto` record should be introduced (there is currently no `AttributeValueDto` at all), mirroring the parameters of the existing `SetProductAttributeValueCommand`:

```csharp
public sealed record CreateAttributeValueDto(
    Guid AttributeId,
    AttributeInputType InputType,
    bool? IsVariantDefiner,
    string? ValueText,
    int? ValueInteger,
    decimal? ValueDecimal,
    bool? ValueBoolean,
    DateTime? ValueDateTime,
    Guid? OptionId
);
```

### 2. Keep the existing single-attribute endpoint for post-creation modifications

This already works well for setting/changing one attribute at a time (e.g., ticking a checkbox on an admin form). Do not remove it — it serves a different use case (incremental edits).

### 3. Add a batch replacement endpoint: `PUT /api/products/{productId:guid}/attributes`

This is the RESTful complement to the single-attribute endpoint. It replaces **all** attribute values on the product (or variant) atomically. Useful for:
- Completing a "wizard" where all attributes are finalized at once
- Bulk import scenarios
- Enforcing required-attribute validation as a hard boundary (reject the PUT if required attributes are missing)

### 4. Why not purely two-step?

- **Two-step (create then set)**: Simpler commands, clear REST sub-resource boundary. But the resource exists in an inconsistent state between calls. If the second request fails, you have an orphan product with missing required attributes. Requires compensating logic or soft-delete rollback.
- **Atomic (optional list in create)**: The resource is born consistent. One API call, one transaction, one round-trip. No orphan window. The **optional** nature preserves backward compatibility — existing integrators are unaffected.
- **The hybrid recommended above** gives you both: atomic creation when attributes are known, incremental editing when they are not.

### 5. Enforce required attributes at the domain/application layer, not at the endpoint level

The handler for `CreateProductCommand` (and the batch `PUT` endpoint) should validate that all `IsRequired = true` attributes for the product's category have been provided. Return a `400 Application.Validation.RequiredAttributeMissing` error if not. This keeps the API flexible while maintaining data integrity — the endpoint accepts attribute values optionally, but the domain rejects incomplete writes.
