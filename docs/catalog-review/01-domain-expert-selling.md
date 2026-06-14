# Domain Expert Review — Selling & Merchandising

**Reviewer:** Aisha — decades in retail selling, e-commerce merchandising, pricing strategy

---

## Strengths: What the Model Gets Right

The product-variant separation is the correct foundation for any serious retail platform. In the real world, customers browse a "product" (e.g., "Nike Air Max 270"), but they buy a specific "variant" (size 10, color Red). Your SKU value object (`Sku.cs`) with proper uppercase-alphanumeric validation and 3–50 character range mirrors real distributor and warehouse SKU conventions. I am also impressed by the `Unit` / `GroupUnit` system (kg, lb, piece, box) with exchange rates and a base unit marker — this is sophisticated and serves industries like grocery, hardware, and wholesale where the same product can be sold by weight, by piece, or by case. The attribute system is well-designed for merchandising: `IsFilterable` on `CategoryAttribute` directly enables faceted navigation, the `AttributeInputType` enum covers the essential data types, and separating `AttributeOption` (the definition of a choice) from `AttributeValue` (the assignment to a variant) is correct. On the content side, SEO metadata fields, primary-image handling, sort orders on categories and images, and the slug value object with sanitization all show awareness of real e-commerce needs.

## Critical Gaps: Pricing, Inventory, and Promotions

The most glaring omission is **pricing**. There is not a single price field anywhere in this model — no `Price`, `ListPrice`, `CostPrice`, `SalePrice`, or `PriceValidUntil`. A catalog without pricing is a brochure, not a selling system. In real retail, pricing lives at the variant level (a size-10 shoe may cost differently than a size-14), and it must support scheduled sales, tiered pricing (bulk discounts), customer-group pricing (wholesale vs. retail), and currency. I would add a `Price` entity or value object on `ProductVariant` with fields like `ListPrice`, `SalePrice`, `CostPrice`, `ValidFrom`, `ValidTo`, and optionally a `PriceListId` for B2B scenarios. Equally critical is **inventory management**. Live e-commerce needs `StockQuantity`, `LowStockThreshold`, `AllowBackorders`, `TrackInventory` (boolean), and `AvailableQuantity` on each variant. Without these, you cannot prevent overselling, show "only 3 left" urgency messages, or manage warehouse allocation. The `Sku` value object exists, so the infrastructure for tracking is there — but the inventory fields themselves are absent. Lastly, there is **no promotion model**. Discount rules, coupon eligibility, buy-one-get-one, bundle discounts, and flash-sale scheduling are core to merchandising. Even a simple `IsOnSale` / `DiscountPercentage` on the variant would be a start, but a dedicated `Promotion` aggregate with rule expressions or a reference to a future Pricing module is needed.

## Missing Product Concepts That Matter for Selling

Several fundamental retail concepts are absent. First, there is no **product type** discriminator. In the real world, you need to distinguish simple products (no variants, direct sell), configurable products (this model's default), bundled products (kit of multiple items sold together), and digital goods (no shipping). The current design forces every product to have variants, which breaks for a simple widget sold as-is. A `ProductType` enum on `Product` would solve this and let you conditionally enforce variant requirements. Second, there is no **brand** entity or field. Brand filtering is one of the top-three ways customers navigate categories on every major e-commerce site (Amazon, Zalando, Shopify). Third, there are no **shipping-related fields** on the variant: `Weight`, `Length`, `Width`, `Height`, `ShippingClass` (e.g., "oversized," "hazardous"). These are essential for real-time carrier rate calculation at checkout. Fourth, there is no **tax category** (`TaxClassId` or `TaxCode` like "standard-rated," "zero-rated," "food," "clothing") — a legal requirement for any real store that collects VAT, GST, or sales tax. Fifth, there are no **product relationship** collections — no `RelatedProducts`, `CrossSellProducts`, `UpSellProducts`, or `Accessories`. These are critical for merchandising: "Customers who bought this also bought..." is the highest-converting recommendation pattern in e-commerce. Sixth, there are no **availability date fields** (`AvailableFrom`, `AvailableUntil`) for seasonal or pre-order merchandise, and no **condition** field for marketplace or second-hand sales (new, used, refurbished). Finally, while `ProductVariant` has an `IsActive` flag, there is no `TrackInventory` or `AllowBackorder` flag that real operations teams rely on to manage supply chains.

## Category Structure: Good Bones, Missing Merchandising Features

The category model is fundamentally sound. The self-referential parent-child hierarchy with `SortOrder` and `IsActive` supports tree navigation, and the ability to attach attributes at the category level with `IsFilterable` is exactly how faceted search should work — attributes cascade to products in that category. However, from a merchandising perspective, a few additions would dramatically improve it. There is no **category image/banner** or **category icon** beyond a single `ImageUrl` — rich media categories (hero banners for seasonal collections) are standard. There is no **display mode** — some categories should show a grid of products, others a list, others a subcategory-only page (common in apparel). There is no `IncludeInMenu` flag to control which categories appear in the top navigation vs. the footer vs. search-only. There is no `PageSize` override (how many products per page for this category). Also, critically, there is no **visibility scope**: categories cannot be restricted to certain sales channels (web vs. mobile app vs. physical store POS). For a multi-channel platform this is essential. The `CategoryAttribute` model is strong, but the `IsRequired` flag on an attribute has no validation enforcement visible at the aggregate level — this could be tightened to guard product creation against missing required attributes.

## Strategic Recommendations

If this domain is to power a real e-commerce platform, I recommend three immediate additions and two longer-term refactors.

**Immediate:**
1. Add a `ProductVariantPrice` value object or embedded entity on `ProductVariant` with `ListPrice`, `SalePrice`, `CostPrice`, and currency fields. Without this, the catalog cannot be sold.
2. Add inventory fields to `ProductVariant`: `StockQuantity`, `LowStockThreshold`, `TrackInventory`, `AllowBackorder`, `AvailableFrom`, `AvailableUntil`. These are non-negotiable for live operations.
3. Introduce a `ProductType` enum on `Product` (Simple, Configurable, Bundle, Digital) and adjust invariants accordingly — for instance, a Simple product should not require variants.

**Medium-term:**
- Introduce a `Brand` aggregate and a `ProductBrand` join to enable brand navigation and filtering.
- Add `ProductRelation` as its own entity with a `RelationType` enum (Related, CrossSell, UpSell, Accessory) to support recommendation merchandising.

**Longer-term:**
- Introduce a `TaxCategory` value object or reference a future Taxing bounded context, and add shipping dimensions (`Weight`, `Length`, `Width`, `Height`) to `ProductVariant` for carrier integration.

The foundation of this code is genuinely good — the attribute system, unit handling, and domain event pattern are well above average for a .NET backend. But a catalog is not ready for selling until prices, stock, and product types are first-class citizens of the model.
