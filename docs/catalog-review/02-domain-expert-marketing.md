# Domain Expert Review — Marketing & SEO

**Reviewer:** Marcus — digital marketing, SEO, product content strategy

---

## 1. SEO Foundation: Solid Basics, Significant Gaps in Sophistication

The model gets the fundamentals right with `Slug` as a formal `ValueObject` (regex-validated, lowercased, hyphenated) and a dedicated SEO triad on `Product` (`MetaTitle`, `MetaDescription`, `MetaKeywords`). This is the skeleton of on-page optimization. The `Slug` construction logic in `Slug.Create()` is good — stripping special characters, collapsing hyphens, enforcing a minimum length — and using it on both `Product` and `Category` sets up clean URL structures.

However, for a real-world e-commerce platform aiming to compete in organic search, several critical SEO primitives are missing. **There is no canonical URL field anywhere in the domain.** This is the single most important omission. Variants, filterable search results (powered by `IsFilterable` attributes), and products accessible through multiple category paths all create duplicate content that will dilute link equity without canonical tags. The `Category` entity has **zero SEO fields** — no `MetaTitle`, `MetaDescription`, `MetaKeywords`, no `Robots` (noindex/nofollow) directive. Categories are often the highest-traffic landing pages in an e-commerce store, and treating them as SEO-citizens is non-negotiable. Also absent: `Hreflang` support (critical for any future multi-region expansion), `OpenGraph` / `Twitter Card` fields for social sharing, and any integration point for JSON-LD structured data markup (schema.org `Product`, `BreadcrumbList`, `Offer`, `AggregateRating`). While structured data can be composed at the API/presentation layer, the domain should ideally carry fields like `Gtin`, `Mpn`, or `Brand` that directly feed those schemas.

## 2. Content & Marketing Fields: Functional but Thin

The image model is well-considered: `ProductImage` and `VariantImage` both carry `AltText`, `IsPrimary`, and `SortOrder`, which gives the marketing team control over image hierarchy and accessibility. The `SortOrder` field on `Category` also allows curated ordering on listing pages. `Product.Details` and `Category.Description` provide free-text content areas.

But the content arsenal stops short of what modern e-commerce marketing requires. There is only a single `Details` field on `Product` — no separation between a **short description** (for listing cards and search snippets) and a **long description** (for the product detail page). This dual-description pattern is standard in platforms like Shopify, Magento, and CommerceTools because it directly impacts click-through rates from search. There are no **badges or labels** — no first-class concept for "New Arrival", "Best Seller", "Limited Edition", "Clearance", or "Eco-Friendly". These badges are the visual currency of promotional campaigns and are referenced across the entire frontend (navigation, listing cards, product detail). Their absence means every badge must be hacked onto the product via naming conventions in `MetaKeywords` or attributes, which is brittle.

**Brand** is another glaring omission. In any product catalog, brand is a core navigation facet, a filter, an SEO signal (brand + product name is a top query pattern), and a content grouping mechanism. Without a `BrandId` or `BrandName` on `Product`, the domain cannot support brand pages, brand-level filtering, or rich snippet markup for `schema.org/Brand`. Likewise, **product tags** (a many-to-many, user-facing keyword system separate from the category tree and `MetaKeywords`) are absent. Tags power marketing segments, blog-to-product linking, recommendation widgets, and seasonal groupings without forcing rigid category restructuring.

## 3. Category Structure: Hierarchical but Campaign-Naive

The `Category` entity has a solid hierarchical model (`ParentId`, `SubCategories`, `SortOrder`, `IsActive`) and supports attribute-driven faceted navigation via `IsFilterable`. This is sufficient for a static taxonomy. But it has no provisions for **marketing campaigns, seasonal pop-ups, or promotional groupings**. Real-world e-commerce teams create temporary structures routinely: a "Summer Sale" category that lives for three months, a "Gifts Under $50" holiday hub, or a "New Arrivals" department that rolls weekly. These require:

- **StartDate / EndDate** for automatic activation and deactivation of time-bound categories.
- A **PromotionType** or **IsCampaign** flag to distinguish permanent taxonomy from ephemeral marketing structures.
- **Visibility overrides** — e.g., show in main navigation only during the campaign window, or restrict to authenticated users for member-exclusive sales.
- A dedicated **Campaign landing page identifier** or template field so marketing can assign a unique layout or hero banner to a promotional category.

Currently, the only way to run a seasonal promotion is to manually create a category, populate it with products, and later manually deactivate it. There is also no **category-level SEO metadata**, which makes seasonal landing pages invisible to search optimization until the marketing team finds a workaround (e.g., storing title tags in `Description`). The `ImageUrl` field conflates thumbnail and hero banner, whereas promotional categories often need multiple image assets (mobile banner, desktop hero, thumbnail, social card).

## 4. Broader Marketing Infrastructure: What the Domain Doesn't Know

Beyond the specific gaps above, several cross-cutting concerns affect real-world marketing execution:

- **Pricing & Promotions** — The domain has no concept of price (list price, sale price, tiered pricing), discount schedules, coupon codes, or buy-one-get-one offers. While pricing may live in a separate aggregate, marketing campaigns are inseparable from pricing logic. A domain event like `ProductPriceChanged` or `PromotionActivated` that the Catalog can react to would be valuable.
- **Product Relationships** — There are no fields or collections for related products, cross-sells, upsells, or bundled SKUs. These drive average order value and are core to merchandising strategy.
- **Content Localization** — None of the content fields (`Name`, `Details`, `Description`, `MetaTitle`, `MetaDescription`, `AltText`) have a locale bag or culture qualifier. If the platform ever expands beyond a single market, every entity will need significant refactoring.
- **Inventory Signals** — No stock status (InStock, OutOfStock, PreOrder, BackOrder). Marketing teams build campaigns around stock availability ("Low Stock!", "Back in Stock", "Pre-order Now"). The domain should carry this state visibly.
- **Review & Rating** — No aggregate review data (average rating, review count) on `Product`. These are powerful SEO signals (stars in SERPs) and conversion drivers.
- **Sitemap & Crawl Controls** — No `IncludeInSitemap`, `Priority`, or `ChangeFreq` on Product or Category. This forces SEO configuration into configuration files rather than making it data-driven.
- **Variant-Level SEO** — Variants share the parent product's slug, but there is no variant-level `MetaTitle` or `MetaDescription`. For SEO-critical variations (different colors, sizes), being able to override page title per variant is important.

## 5. Overall Assessment

**Strengths.** The Catalog domain is cleanly factored, follows DDD conventions well, and has a thoughtful attribute system (`CategoryAttribute` + `AttributeValue` with type-safe value fields, `IsFilterable`, `AttributeInputType` enum, `AttributeOption` for multi-select). The `Slug` value object is robust. The `Unit` / `GroupUnit` subsystem shows foresight for weighted or volumetric products. Image management with `AltText`, `SortOrder`, and `IsPrimary` on both product and variant images is best practice. The domain events (`ProductCreatedDomainEvent`, `ProductUpdatedDomainEvent`, etc.) provide extension points for search indexing, sitemap regeneration, and cache invalidation.

**Critical Gaps.** For a production e-commerce catalog that must support a modern marketing and SEO strategy, the model needs:
1. Canonical URL support
2. SEO metadata on `Category` (title, description, keywords, robots)
3. Brand as a first-class entity or value
4. Product tags for flexible categorization
5. A badge/label system for promotional indicators
6. Separate short and long description fields
7. Temporal awareness for time-bound campaigns
8. Structured data primitives (GTIN, MPN, brand) to feed schema.org markup

The attribute system is flexible enough to approximate some of these needs, but doing so would misuse `AttributeValue` for marketing metadata that deserves its own semantics, query paths, and lifecycle.

**Recommendation.** The core architecture is sound and the team has made good decisions about aggregate boundaries and value objects. I recommend treating the SEO and marketing gaps as a prioritized backlog: start with canonical URLs and Category SEO (low effort, high SEO impact), then add brand and product tags (medium effort, unlocks navigation and filtering), then tackle badges and campaign-aware categories (higher effort, enables marketing autonomy). The domain events already in place make it feasible to add these fields without breaking existing consumers, provided careful stewardship of the outbox and search reindexing pipelines.
