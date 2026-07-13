using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.Commands.Login;
using AE.Market.Application.Features.Catalog.Commands.AddProductVariant;
using AE.Market.Application.Features.Catalog.Commands.CreateProduct;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Pricing.Commands.SetInitialPrice;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Pricing;
using AE.Market.Domain.Common.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace AE.Market.Integration.Tests;

[Collection("Integration tests")]
public sealed class PriceCalculatorIntegrationTests(IntegrationTestWebAppFactory factory)
{
    private readonly HttpClient _client = factory.HttpClient;
    private const string AuthBase = "api/auth";
    private const string CategoriesBase = "api/categories";
    private const string BrandsBase = "api/brands";
    private const string ProductsBase = "api/products";
    private const string TaxCodesBase = "api/product-tax-codes";

    private async Task<string> GetAdminTokenAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync($"{AuthBase}/login",
            new LoginCommand("admin@aemarket.com", "Admin@12345"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokensResponseDto>();
        return tokens!.AccessToken;
    }

    private void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private void ClearAuthHeader()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<(Guid ProductId, Guid VariantId, Guid TaxCodeId)> CreateProductWithVariantAsync(string adminToken)
    {
        SetAuthHeader(adminToken);

        var allCats = await (await _client.GetAsync(CategoriesBase))
            .Content.ReadFromJsonAsync<PaginatedList<CategoryDto>>();
        var allBrands = await (await _client.GetAsync(BrandsBase))
            .Content.ReadFromJsonAsync<PaginatedList<BrandDto>>();
        var allTaxCodes = await (await _client.GetAsync(TaxCodesBase))
            .Content.ReadFromJsonAsync<List<ProductTaxCodeDto>>();

        var categoryId = allCats!.Items.First().Id;
        var brandId = allBrands!.Items.First().Id;
        var taxCodeId = allTaxCodes!.First().Id;

        var prodName = $"PriceCalcProd-{Guid.NewGuid():N}";
        var prodCmd = new CreateProductCommand(
            prodName, prodName.ToLower(), $"SKU-{Guid.NewGuid():N}",
            null, null, null,
            categoryId, brandId, taxCodeId,
            "Simple", false, null);
        var prodResponse = await _client.PostAsJsonAsync(ProductsBase, prodCmd);
        prodResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await prodResponse.Content.ReadFromJsonAsync<ProductDto>();

        var varCmd = new AddProductVariantCommand(product!.Id, $"Variant-{Guid.NewGuid():N}", $"VAR-{Guid.NewGuid():N}");
        var varResponse = await _client.PostAsJsonAsync($"{ProductsBase}/{product.Id}/variants", varCmd);
        varResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var variant = await varResponse.Content.ReadFromJsonAsync<VariantDto>();

        ClearAuthHeader();
        return (product.Id, variant!.Id, taxCodeId);
    }

    private async Task<Guid> GetGlobalMarketplaceIdAsync()
    {
        using var scope = factory.CreateScope();
        var marketplaceRepo = scope.ServiceProvider.GetRequiredService<IReadRepository<Marketplace>>();
        var marketplace = await marketplaceRepo.FirstOrDefaultAsync(
            new MarketplaceByCodeSpec("global"));
        return marketplace!.Id;
    }

    [Fact]
    public async Task CalculateAsync_WithActiveSalePrice_ShouldReturnCorrectBreakdown()
    {
        var adminToken = await GetAdminTokenAsync();
        var (_, variantId, _) = await CreateProductWithVariantAsync(adminToken);
        var marketplaceId = await GetGlobalMarketplaceIdAsync();

        var salePrice = 49.99m;
        SetAuthHeader(adminToken);
        var setPriceCmd = new SetInitialPriceCommand(
            variantId, marketplaceId, PriceType.Sale, salePrice, "USD");
        var priceResponse = await _client.PostAsJsonAsync(
            $"api/products/{Guid.NewGuid()}/variants/{variantId}/price", setPriceCmd);
        priceResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        ClearAuthHeader();

        using var scope = factory.CreateScope();
        var calculator = scope.ServiceProvider.GetRequiredService<IPriceCalculator>();

        var result = await calculator.CalculateAsync(variantId, 2, marketplaceId);

        result.Should().NotBeNull();
        result.VariantId.Should().Be(variantId);
        result.Quantity.Should().Be(2);
        result.UnitPrice.Should().NotBeNull();
        result.UnitPrice.Currency.Code.Should().Be("USD");
        result.TotalPrice.Should().NotBeNull();
        result.TotalPrice.Currency.Code.Should().Be("USD");
        result.Breakdown.Should().NotBeEmpty();

        var baseItem = result.Breakdown.Should()
            .ContainSingle(b => b.Type == PriceBreakdownType.Base).Subject;
        baseItem.Amount.Amount.Should().Be(salePrice);
        baseItem.Label.Should().Be("Base Price");

        var taxItem = result.Breakdown.Should()
            .ContainSingle(b => b.Type == PriceBreakdownType.Tax).Subject;
        taxItem.Amount.Amount.Should().Be(salePrice * 0.20m);

        result.UnitPrice.Amount.Should().BeGreaterThan(salePrice);
        result.TotalPrice.Amount.Should().Be(result.UnitPrice.Amount * 2);
    }

    [Fact]
    public async Task CalculateAsync_WithoutActivePrice_ShouldFallBackToListPrice()
    {
        var adminToken = await GetAdminTokenAsync();
        var (_, variantId, _) = await CreateProductWithVariantAsync(adminToken);
        var marketplaceId = await GetGlobalMarketplaceIdAsync();

        using (var scope = factory.CreateScope())
        {
            var variantRepo = scope.ServiceProvider
                .GetRequiredService<IRepository<AE.Market.Domain.Aggregates.Catalog.Products.Variants.ProductVariant>>();
            var variant = await variantRepo.GetByIdAsync(variantId);
            variant.Should().NotBeNull();
        }

        using var calcScope = factory.CreateScope();
        var calculator = calcScope.ServiceProvider.GetRequiredService<IPriceCalculator>();

        var result = await calculator.CalculateAsync(variantId, 1, marketplaceId);

        result.Should().NotBeNull();
        result.VariantId.Should().Be(variantId);
        result.Quantity.Should().Be(1);
        result.UnitPrice.Amount.Should().Be(0.49m);
        result.TotalPrice.Amount.Should().Be(0.49m);
        result.Breakdown.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CalculateAsync_WithNonExistentMarketplace_ShouldReturnZero()
    {
        var adminToken = await GetAdminTokenAsync();
        var (_, variantId, _) = await CreateProductWithVariantAsync(adminToken);

        using var scope = factory.CreateScope();
        var calculator = scope.ServiceProvider.GetRequiredService<IPriceCalculator>();

        var result = await calculator.CalculateAsync(variantId, 1, Guid.NewGuid());

        result.Should().NotBeNull();
        result.UnitPrice.Amount.Should().Be(0m);
        result.TotalPrice.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_WithNonExistentVariant_ShouldReturnZero()
    {
        var marketplaceId = await GetGlobalMarketplaceIdAsync();

        using var scope = factory.CreateScope();
        var calculator = scope.ServiceProvider.GetRequiredService<IPriceCalculator>();

        var result = await calculator.CalculateAsync(Guid.NewGuid(), 1, marketplaceId);

        result.Should().NotBeNull();
        result.UnitPrice.Amount.Should().Be(0m);
        result.TotalPrice.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_WithQuantity_ShouldMultiplyTotalPrice()
    {
        var adminToken = await GetAdminTokenAsync();
        var (_, variantId, _) = await CreateProductWithVariantAsync(adminToken);
        var marketplaceId = await GetGlobalMarketplaceIdAsync();

        var salePrice = 25.00m;
        SetAuthHeader(adminToken);
        var setPriceCmd = new SetInitialPriceCommand(
            variantId, marketplaceId, PriceType.Sale, salePrice, "USD");
        var priceResponse = await _client.PostAsJsonAsync(
            $"api/products/{Guid.NewGuid()}/variants/{variantId}/price", setPriceCmd);
        priceResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        ClearAuthHeader();

        using var scope = factory.CreateScope();
        var calculator = scope.ServiceProvider.GetRequiredService<IPriceCalculator>();

        var result = await calculator.CalculateAsync(variantId, 5, marketplaceId);

        result.TotalPrice.Amount.Should().Be(result.UnitPrice.Amount * 5);
        result.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task CalculateAsync_Rounding_ShouldProduceRoundingItem()
    {
        var adminToken = await GetAdminTokenAsync();
        var (_, variantId, _) = await CreateProductWithVariantAsync(adminToken);
        var marketplaceId = await GetGlobalMarketplaceIdAsync();

        var salePrice = 13.33m;
        SetAuthHeader(adminToken);
        var setPriceCmd = new SetInitialPriceCommand(
            variantId, marketplaceId, PriceType.Sale, salePrice, "USD");
        var priceResponse = await _client.PostAsJsonAsync(
            $"api/products/{Guid.NewGuid()}/variants/{variantId}/price", setPriceCmd);
        priceResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        ClearAuthHeader();

        using var scope = factory.CreateScope();
        var calculator = scope.ServiceProvider.GetRequiredService<IPriceCalculator>();

        var result = await calculator.CalculateAsync(variantId, 1, marketplaceId);

        var afterTax = salePrice + (salePrice * 0.20m);
        result.Breakdown.Should().Contain(b => b.Type == PriceBreakdownType.Rounding);
        result.UnitPrice.Amount.Should().NotBe(afterTax);
    }

    private sealed record TokensResponseDto(string AccessToken, string RefreshToken, Guid? UserId);
}
