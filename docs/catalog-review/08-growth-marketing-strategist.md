# Growth Marketing Strategist — Catalog UX & Analytics Review

## Summary

| Severity | Count |
|----------|-------|
| Critical | 6 |
| Major | 15 |
| Minor | 4 |
| Info | 2 |

---

## Critical

### 1. No full-text / keyword search
`GetProductsListQuery.cs:6-12` — Only `IsActive` + `SortBy` + pagination; no `SearchTerm` filter.

### 2. No demo products seeded
`DbSeeder.cs:35-75` — Creates categories/brands/tax codes but zero products. First-time visitors see empty catalog.

### 3. No `ProductViewedDomainEvent`
`GetProductByIdQueryHandler.cs` — Product detail retrieval fires no analytics signal.

### 4. No `ProductSearchedDomainEvent`
No event on search query. Cannot track zero-result searches or popular queries.

### 5. No wishlist/favorites  
No `WishlistItem` entity, command, or query for bookmarking products.
> **TODO:** The plan to create `Engagment Aggregate` the holds entities like `WishListItem`, `FavoritList` and `FavoriteListItem`... 


### 6. No product reviews/ratings [Keep it for Social Aggregate]
No `Review` aggregate — no social proof mechanism.
> **TODO:** The plan the create `Social Aggregate` to manage Reviews and Rating (`ProductId`, `CustomerId`, `Rating`, `Title`, `Body` ....)
> In `Prouct` we add two lightweight readonly fields (`AverageRating`, `TotalReviewsCount`) 
>Whenever a customer drop a review `Social Sggregate` recalculate the average and the count and  rise domain event `ProductReviewSummaryUpdatedEvent` so `Catalog Aggregate` listen to this event and update its fields 

---

## Major

### Product Discovery
- **No attribute-based filtering** — no price range, attribute values on list endpoints
- **No faceted search** — no aggregation counts (total by category, brand, price buckets)
- **GET /api/products** has no `[FromQuery]` binding — potentially breaks model binding

### Category / Brand Navigation
- **No slug-based lookup** for categories or brands (`GET /slug/{slug}` missing)
- **Categories list is flat** — no tree structure, no `?parentId=` filter
- **Brands list has no pagination**

### Seed Data
- `ProductTaxCodeSeeder.cs` — Dead code, never called
- **No demo variant images** — seeded products need at least one image per variant

### Analytics Hooks
- **No `CartAddAnalyticsEvent`** — no event for "added to cart"
- **Event payloads are anemic** — `ProductCreatedDomainEvent`, `ProductUpdatedDomainEvent`, `CategoryCreatedDomainEvent` carry only aggregate ID
- **No UTM/referral tracking** — no fields on queries or events
- **Event handlers only do cache invalidation** — no analytics persistence or webhook dispatch

### Conversion Funnel
- **`AvailableQuantity` not exposed in `VariantDto`** — frontend can't show "Only 3 left"
- **No `IsInStock` / `IsLowStock` computed field on `ProductDto`**
- **No low-stock domain event** — no back-in-stock notification trigger
- **No recently viewed products** — no "continue browsing" query

---

## Minor

### 2.3 — Brands list no pagination (acceptable for few brands)
### 2.4 — No category breadcrumb endpoint
### 5.4 — `AllowBackOrder` / `BackOrderLimit` not in `ProductDetailDto`
### 6.5 — No product comparison endpoint

---

## Info

### 3.4 — No first-run `/api/catalog/status` endpoint
### 5.5 — `ReservedQuantity` optionally exposable in `VariantDto`
