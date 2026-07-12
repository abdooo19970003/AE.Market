# Frontend / UX Designer — Required Attribute Enforcement Decision

## Current State (Gaps Found)

| Aspect | Status |
|---|---|
| `GET /api/products/{productId}/attributes` | Not wired to a controller — the `GetRequiredAttributesForProductQuery` handler exists but has **no HTTP endpoint**. The frontend cannot currently discover required attributes. |
| `POST /api/products` (CreateProduct) | Accepts **zero** attribute values. Only base fields (name, slug, sku, category, etc.). |
| `POST /api/products/{productId}/variants` (AddVariant) | Accepts **zero** attribute values. Only name + sku. |
| `POST /api/products/{productId}/attributes` (SetProductAttributeValue) | One attribute at a time. No batch endpoint. |
| Validation error messages | FluentValidation returns generic `"Validation failed"` via `ProblemDetails`. No structured way to say *"Attributes X, Y, Z are required"*. |
| `CategoryDto` | Does **not** include attributes — the frontend cannot eagerly load them from the category endpoint. |
| Input types | `Text`, `Integer`, `Decimal`, `MultiSelect`, `Boolean`, `DateTime` — each with different payload shapes. |

---

## Recommended API Flow (Frontend Perspective)

The ideal UX flow is:

```
User picks a category
       |
       v
Frontend calls GET /api/categories/{id}/attributes?requiredOnly=true
  → receives list of attributes with InputType, Options, IsRequired
       |
       v
Frontend renders dynamic form fields (textbox, dropdown, checkbox, datepicker, etc.)
       |
       v
User fills in values
       |
       v
Frontend sends POST /api/products with attributes inline
  → server validates required attributes are present
  → returns 201 with full product detail (including attribute values)
```

---

## Recommendations (5 Bullet Points)

### 1. Expose a required-attributes endpoint and wire it to the controller

The query `GetRequiredAttributesForProductQuery` already exists with caching and handles `CategoryId` + optional `ProductId` (for edit mode). It just needs a controller route:

```
GET /api/categories/{categoryId}/required-attributes?productId={guid?}
```

This returns `RequiredAttributeDto[]` with `AttributeId`, `AttributeName`, `InputType`, `IsRequired`, `IsVariantDefiner`, and `CurrentValue` (for edit scenarios). The frontend can use this to dynamically render inputs (text fields for `Text`, dropdowns for `MultiSelect` using `AttributeOptionDto`, checkboxes for `Boolean`, number inputs for `Integer`/`Decimal`, date pickers for `DateTime`).

The `CategoryDto` should also include a flat `Attributes: CategoryAttributeDto[]` property so the frontend can batch-load this data when fetching category details.

### 2. Inline attribute values in create endpoints (batch setting)

Add an `AttributeValues` collection to both `CreateProductCommand` and `AddProductVariantCommand`:

```csharp
// Inside CreateProductCommand
IReadOnlyList<AttributeValueInput>? AttributeValues

// Reusable DTO
public sealed record AttributeValueInput
{
    public Guid AttributeId { get; init; }
    public string? ValueText { get; init; }
    public int? ValueInteger { get; init; }
    public decimal? ValueDecimal { get; init; }
    public bool? ValueBoolean { get; init; }
    public DateTime? ValueDateTime { get; init; }
    public Guid? OptionId { get; init; }
    public bool? IsVariantDefiner { get; init; }
}
```

The handler loops and calls `product.SetAttributeValue(...)` for each entry. This eliminates the two-step create-then-set pattern entirely. The `AttributeInputType` can be inferred server-side from `CategoryAttribute.InputType` using `AttributeId`.

### 3. Structured error responses for missing required attributes

Today's validation pipeline returns generic 400 with `"Validation failed"`. The frontend needs to know **which** attributes are missing to highlight the correct form fields.

Enhance the FluentValidation (or add a post-validation step in the handler) to check required attributes for the given `CategoryId`. Return a structured error envelope:

```json
{
  "type": "Application.Validation",
  "title": "Validation Failed",
  "status": 400,
  "errors": {
    "AttributeValues": [
      "Attribute 'Screen Size' (ID: ...) is required.",
      "Attribute 'Color' (ID: ...) is required."
    ]
  },
  "missingAttributeIds": ["guid-1", "guid-2"]
}
```

The frontend can pattern-match on `missingAttributeIds` to highlight the specific form fields. The `Result` pattern in this project allows returning custom error codes — use `MissingRequiredAttributes` with structured payload.

### 4. Add a batch update endpoint for attribute values

For the edit product flow (not just create), add:

```
PUT /api/products/{productId}/attributes
Body: { values: AttributeValueInput[] }
```

This replaces all attribute values in one call, rather than forcing N individual `POST /api/products/{productId}/attributes` calls. The handler clears existing values and re-adds them (or upserts).

### 5. Include attribute values in read-model responses

Currently `ProductDetailDto` and `ProductDto` do **not** include attribute values. The frontend needs them to render the product detail/edit page. Add a `List<RequiredAttributeDto> Attributes` to `ProductDetailDto` so the frontend can hydrate edit forms from a single `GET /api/products/{id}` call instead of making a separate required-attributes query.

---

## Summary of the Ideal Frontend Flow

```
1. GET /api/categories                          → pick category
2. GET /api/categories/{id}                     → includes attributes with InputType + Options
3. Frontend renders form: textboxes, dropdowns, checkboxes, date pickers
4. POST /api/products  { ..., attributeValues: [...] }
   → On 201: Product created with all attributes set
   → On 400: structured errors with missingAttributeIds to highlight fields
5. Edit: GET /api/products/{id}  → includes current attribute values
6. Edit: PUT /api/products/{id}/attributes  → batch update all
```

This eliminates the awkward two-step create-then-set pattern and gives the frontend everything it needs to render the correct form controls for each attribute type.
