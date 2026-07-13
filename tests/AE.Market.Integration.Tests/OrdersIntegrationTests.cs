using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AE.Market.Application.Features.Auth.Commands.Login;
using AE.Market.Application.Features.Auth.Commands.Register;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Cart.Commands.AddToCart;
using AE.Market.Application.Features.Catalog.Commands.AddProductVariant;
using AE.Market.Application.Features.Catalog.Commands.CreateBrand;
using AE.Market.Application.Features.Catalog.Commands.CreateCategory;
using AE.Market.Application.Features.Catalog.Commands.CreateProduct;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Orders.DTOs;
using FluentAssertions;

namespace AE.Market.Integration.Tests;

[Collection("Integration tests")]
public sealed class OrdersIntegrationTests(IntegrationTestWebAppFactory factory)
{
    private readonly HttpClient _client = factory.HttpClient;
    private const string AuthBase = "api/auth";
    private const string CartBase = "api/cart";
    private const string OrdersBase = "api/orders";
    private const string CategoriesBase = "api/categories";
    private const string BrandsBase = "api/brands";
    private const string ProductsBase = "api/products";
    private const string TaxCodesBase = "api/product-tax-codes";

    private async Task<string> RegisterAndLoginAsync()
    {
        var email = $"order-test-{Guid.NewGuid():N}@example.com";
        var registerCmd = new RegisterCommand(email, "Password123");
        var regResponse = await _client.PostAsJsonAsync($"{AuthBase}/register", registerCmd);
        regResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginCmd = new LoginCommand(email, "Password123");
        var loginResponse = await _client.PostAsJsonAsync($"{AuthBase}/login", loginCmd);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokensResponseDto>();
        return tokens!.AccessToken;
    }

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

    private async Task<Guid> CreateSampleVariantAsync(string adminToken)
    {
        SetAuthHeader(adminToken);

        var allCats = await (await _client.GetAsync(CategoriesBase)).Content.ReadFromJsonAsync<PaginatedList<CategoryDto>>();
        var allBrands = await (await _client.GetAsync(BrandsBase)).Content.ReadFromJsonAsync<PaginatedList<BrandDto>>();
        var allTaxCodes = await (await _client.GetAsync(TaxCodesBase)).Content.ReadFromJsonAsync<List<ProductTaxCodeDto>>();

        var productCmd = new CreateProductCommand(
            $"Sample-{Guid.NewGuid():N}",
            $"sample-{Guid.NewGuid():N}",
            $"SKU-{Guid.NewGuid():N}",
            null, null, null,
            allCats!.Items.First().Id,
            allBrands!.Items.First().Id,
            allTaxCodes!.First().Id,
            "Simple", false, null);
        var productResponse = await _client.PostAsJsonAsync(ProductsBase, productCmd);
        productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();

        var variantCmd = new AddProductVariantCommand(product!.Id, "Test Variant", $"VAR-{Guid.NewGuid():N}");
        var variantResponse = await _client.PostAsJsonAsync($"{ProductsBase}/{product.Id}/variants", variantCmd);
        variantResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var variant = await variantResponse.Content.ReadFromJsonAsync<VariantDto>();

        ClearAuthHeader();
        return variant!.Id;
    }

    [Fact]
    public async Task PlaceOrder_WithValidCart_ShouldReturnCreated()
    {
        var token = await RegisterAndLoginAsync();
        var adminToken = await GetAdminTokenAsync();
        var variantId = await CreateSampleVariantAsync(adminToken);
        SetAuthHeader(token);

        var addItemCmd = new AddToCartCommand(null, null, variantId, 2);
        var addResponse = await _client.PostAsJsonAsync($"{CartBase}/items", addItemCmd);
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var orderResponse = await _client.PostAsync(OrdersBase, null);

        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.OrderNumber.Should().NotBeNullOrEmpty();
        order.Items.Should().HaveCount(1);
        order.Items[0].VariantId.Should().Be(variantId);
        order.Items[0].Quantity.Should().Be(2);

        ClearAuthHeader();
    }

    [Fact]
    public async Task PlaceOrder_DuplicateIdempotencyKey_ShouldReturnSameOrder()
    {
        var token = await RegisterAndLoginAsync();
        var adminToken = await GetAdminTokenAsync();
        var variantId = await CreateSampleVariantAsync(adminToken);
        SetAuthHeader(token);

        var addItemCmd = new AddToCartCommand(null, null, variantId, 1);
        await _client.PostAsJsonAsync($"{CartBase}/items", addItemCmd);

        var idempotencyKey = Guid.NewGuid().ToString();
        _client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

        var firstResponse = await _client.PostAsync(OrdersBase, null);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var firstOrder = await firstResponse.Content.ReadFromJsonAsync<OrderDto>();

        var secondResponse = await _client.PostAsync(OrdersBase, null);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var secondOrder = await secondResponse.Content.ReadFromJsonAsync<OrderDto>();

        secondOrder!.OrderNumber.Should().Be(firstOrder!.OrderNumber);

        ClearAuthHeader();
    }

    [Fact]
    public async Task GetOrderHistory_ShouldReturnUserOrders()
    {
        var token = await RegisterAndLoginAsync();
        var adminToken = await GetAdminTokenAsync();
        var variantId = await CreateSampleVariantAsync(adminToken);
        SetAuthHeader(token);

        var addItemCmd = new AddToCartCommand(null, null, variantId, 1);
        await _client.PostAsJsonAsync($"{CartBase}/items", addItemCmd);
        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        await _client.PostAsync(OrdersBase, null);

        _client.DefaultRequestHeaders.Remove("Idempotency-Key");

        var historyResponse = await _client.GetAsync(OrdersBase);
        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await historyResponse.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders!.Count.Should().BeGreaterThanOrEqualTo(1);

        ClearAuthHeader();
    }

    [Fact]
    public async Task PlaceOrder_WithoutItemsInCart_ShouldReturnBadRequest()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthHeader(token);

        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var orderResponse = await _client.PostAsync(OrdersBase, null);

        orderResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        ClearAuthHeader();
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnOrder()
    {
        var token = await RegisterAndLoginAsync();
        var adminToken = await GetAdminTokenAsync();
        var variantId = await CreateSampleVariantAsync(adminToken);
        SetAuthHeader(token);

        await _client.PostAsJsonAsync($"{CartBase}/items", new AddToCartCommand(null, null, variantId, 3));
        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var createResponse = await _client.PostAsync(OrdersBase, null);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        _client.DefaultRequestHeaders.Remove("Idempotency-Key");

        var getResponse = await _client.GetAsync($"{OrdersBase}/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        fetched!.Id.Should().Be(created.Id);

        ClearAuthHeader();
    }

    [Fact]
    public async Task CancelOrder_ShouldSetStatusToCancelled()
    {
        var token = await RegisterAndLoginAsync();
        var adminToken = await GetAdminTokenAsync();
        var variantId = await CreateSampleVariantAsync(adminToken);
        SetAuthHeader(token);

        await _client.PostAsJsonAsync($"{CartBase}/items", new AddToCartCommand(null, null, variantId, 1));
        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var createResponse = await _client.PostAsync(OrdersBase, null);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        _client.DefaultRequestHeaders.Remove("Idempotency-Key");

        var cancelResponse = await _client.PostAsync($"{OrdersBase}/{created!.Id}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<OrderDto>();
        cancelled!.Status.Should().Be("Cancelled");

        ClearAuthHeader();
    }

    [Fact]
    public async Task PlaceOrder_WithoutAuth_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var response = await _client.PostAsync(OrdersBase, null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _client.DefaultRequestHeaders.Remove("Idempotency-Key");
    }
}
