using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Pricing;
using AE.Market.Domain.Common.ValueObjects;

namespace AE.Market.Application.Features.Pricing.Services;

internal sealed class PriceCalculatorService(
    IReadRepository<Price> priceRepo,
    IReadRepository<Marketplace> marketplaceRepo,
    IReadRepository<MarketplaceTaxRate> taxRateRepo,
    IReadRepository<ProductVariant> variantRepo,
    IReadRepository<Product> productRepo
) : IPriceCalculator
{
    public async Task<FinalPrice> CalculateAsync(
        Guid variantId,
        int quantity,
        Guid marketplaceId,
        CancellationToken ct = default)
    {
        var breakdown = new List<PriceBreakdownItem>();

        var marketplace = await marketplaceRepo.FirstOrDefaultAsync(
            new MarketplaceByIdSpec(marketplaceId), ct);

        if (marketplace is null)
        {
            return new FinalPrice
            {
                VariantId = variantId,
                Quantity = quantity,
                UnitPrice = Money.Zero(Currency.USD),
                TotalPrice = Money.Zero(Currency.USD),
                Breakdown = breakdown,
            };
        }

        var currency = marketplace.PreferredCurrency;

        var variant = await variantRepo.GetByIdAsync(variantId, ct);

        if (variant is null)
        {
            return new FinalPrice
            {
                VariantId = variantId,
                Quantity = quantity,
                UnitPrice = Money.Zero(currency),
                TotalPrice = Money.Zero(currency),
                Breakdown = breakdown,
            };
        }

        var product = await productRepo.GetByIdAsync(variant.ProductId, ct);

        if (product is null)
        {
            return new FinalPrice
            {
                VariantId = variantId,
                Quantity = quantity,
                UnitPrice = Money.Zero(currency),
                TotalPrice = Money.Zero(currency),
                Breakdown = breakdown,
            };
        }

        var activePrice = await priceRepo.FirstOrDefaultAsync(
            new ActivePriceByVariantSpec(variantId, marketplaceId), ct);

        Money currentPrice;
        if (activePrice is not null)
        {
            currentPrice = activePrice.PriceAmount;
        }
        else
        {
            currentPrice = Money.Create(variant.ListPrice, currency);
        }

        breakdown.Add(new PriceBreakdownItem
        {
            Label = "Base Price",
            Amount = currentPrice,
            Type = PriceBreakdownType.Base,
        });

        var taxRate = await taxRateRepo.FirstOrDefaultAsync(
            new ActiveMarketplaceTaxRateSpec(marketplaceId, product.TaxCodeId), ct);

        if (taxRate is not null && taxRate.TaxRate > 0)
        {
            var taxAmount = currentPrice * taxRate.TaxRate;
            currentPrice = currentPrice + taxAmount;
            breakdown.Add(new PriceBreakdownItem
            {
                Label = $"Tax ({taxRate.TaxRate:P0})",
                Amount = taxAmount,
                Type = PriceBreakdownType.Tax,
            });
        }

        var roundedPrice = ApplyRounding(currentPrice);
        var roundingDiff = roundedPrice - currentPrice;
        if (roundingDiff.Amount != 0)
        {
            currentPrice = roundedPrice;
            breakdown.Add(new PriceBreakdownItem
            {
                Label = "Rounding",
                Amount = roundingDiff,
                Type = PriceBreakdownType.Rounding,
            });
        }

        return new FinalPrice
        {
            VariantId = variantId,
            Quantity = quantity,
            UnitPrice = currentPrice,
            TotalPrice = currentPrice * quantity,
            Breakdown = breakdown,
        };
    }

    private static Money ApplyRounding(Money price)
    {
        var amount = price.Amount;
        var floor = Math.Floor(amount);
        var rounded = amount >= floor + 0.5m
            ? floor + 0.99m
            : floor + 0.49m;
        return Money.Create(rounded, price.Currency);
    }
}
