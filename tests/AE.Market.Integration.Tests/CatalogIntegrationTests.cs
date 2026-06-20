using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AE.Market.Application.Features.Auth.Commands.Login;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Catalog.Commands.AddProductVariant;
using AE.Market.Application.Features.Catalog.Commands.CreateBrand;
using AE.Market.Application.Features.Catalog.Commands.CreateCategory;
using AE.Market.Application.Features.Catalog.Commands.CreateProduct;
using AE.Market.Application.Features.Catalog.Commands.UpdateBrand;
using AE.Market.Application.Features.Catalog.Commands.UpdateCategory;
using AE.Market.Application.Features.Catalog.Commands.UpdateProduct;
using AE.Market.Application.Features.Catalog.DTOs;
using FluentAssertions;

namespace AE.Market.Integration.Tests;

[Collection("Integration tests")]
public sealed class CatalogIntegrationTests(IntegrationTestWebAppFactory factory)
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

    // ─────────────────────────────────────────────
    // Categories
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetCategories_ShouldReturnSeedData()
    {
        var response = await _client.GetAsync(CategoriesBase);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<PaginatedList<CategoryDto>>();
        categories.Should().NotBeNull();
        categories!.Items.Select(c => c.CategoryName).Should().Contain(["Electronics", "Clothing", "Books"]);
    }

    [Fact]
    public async Task GetCategoryById_WithExistingId_ShouldReturnOk()
    {
        var allResponse = await _client.GetAsync(CategoriesBase);
        var all = await allResponse.Content.ReadFromJsonAsync<PaginatedList<CategoryDto>>();
        var existingId = all!.Items.First().Id;

        var response = await _client.GetAsync($"{CategoriesBase}/{existingId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        category.Should().NotBeNull();
        category!.Id.Should().Be(existingId);
    }

    [Fact]
    public async Task GetCategoryById_WithNonExistentId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"{CategoriesBase}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCategory_WithoutAuth_ShouldReturnUnauthorized()
    {
        var command = new CreateCategoryCommand("Test", "test", null, null, null, 0);

        var response = await _client.PostAsJsonAsync(CategoriesBase, command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCategory_WithAdminAuth_ShouldReturnCreated()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var name = $"TestCat-{Guid.NewGuid():N}";
        var command = new CreateCategoryCommand(name, name.ToLower(), "Integration test", null, null, 0);

        var response = await _client.PostAsJsonAsync(CategoriesBase, command);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected 201 but got {(int)response.StatusCode}. Body: {body}");
        }
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        category.Should().NotBeNull();
        category!.CategoryName.Should().Be(name);
        category.Slug.Should().Be(name.ToLower());

        ClearAuthHeader();
    }

    [Fact]
    public async Task CreateCategory_WithParent_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var allResponse = await _client.GetAsync(CategoriesBase);
        var all = await allResponse.Content.ReadFromJsonAsync<PaginatedList<CategoryDto>>();
        var electronics = all!.Items.First(c => c.CategoryName == "Electronics");

        var name = $"SubCat-{Guid.NewGuid():N}";
        var command = new CreateCategoryCommand(name, name.ToLower(), null, electronics.Id, null, 1);

        var response = await _client.PostAsJsonAsync(CategoriesBase, command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        category.Should().NotBeNull();
        category!.ParentId.Should().Be(electronics.Id);
        category.Path.Should().StartWith(electronics.Path);

        ClearAuthHeader();
    }

    [Fact]
    public async Task UpdateCategory_WithAdminAuth_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var name = $"UpdCat-{Guid.NewGuid():N}";
        var createCmd = new CreateCategoryCommand(name, name.ToLower(), null, null, null, 0);
        var createResponse = await _client.PostAsJsonAsync(CategoriesBase, createCmd);
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryDto>();

        var updateCmd = new UpdateCategoryCommand(created!.Id, "Updated-" + name, "Updated desc", null, 5);
        var response = await _client.PutAsJsonAsync($"{CategoriesBase}/{created.Id}", updateCmd);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CategoryDto>();
        updated!.CategoryName.Should().Be("Updated-" + name);
        updated.Description.Should().Be("Updated desc");

        ClearAuthHeader();
    }

    [Fact]
    public async Task DeleteCategory_WithAdminAuth_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var name = $"DelCat-{Guid.NewGuid():N}";
        var createCmd = new CreateCategoryCommand(name, name.ToLower(), null, null, null, 0);
        var createResponse = await _client.PostAsJsonAsync(CategoriesBase, createCmd);
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryDto>();

        var response = await _client.DeleteAsync($"{CategoriesBase}/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"{CategoriesBase}/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        ClearAuthHeader();
    }

    // ─────────────────────────────────────────────
    // Brands
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetBrands_ShouldReturnSeedData()
    {
        var response = await _client.GetAsync(BrandsBase);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var brands = await response.Content.ReadFromJsonAsync<PaginatedList<BrandDto>>();
        brands.Should().NotBeNull();
        brands!.Items.Select(b => b.Name).Should().Contain(["Xiaomi", "Samsung", "Nike"]);
    }

    [Fact]
    public async Task GetBrandById_WithExistingId_ShouldReturnOk()
    {
        var allResponse = await _client.GetAsync(BrandsBase);
        var all = await allResponse.Content.ReadFromJsonAsync<PaginatedList<BrandDto>>();
        var existingId = all!.Items.First().Id;

        var response = await _client.GetAsync($"{BrandsBase}/{existingId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var brand = await response.Content.ReadFromJsonAsync<BrandDto>();
        brand.Should().NotBeNull();
        brand!.Id.Should().Be(existingId);
    }

    [Fact]
    public async Task GetBrandById_WithNonExistentId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"{BrandsBase}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBrand_WithoutAuth_ShouldReturnUnauthorized()
    {
        var command = new CreateBrandCommand("TestBrand", "test-brand", null, null, null, null, 0);

        var response = await _client.PostAsJsonAsync(BrandsBase, command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateBrand_WithAdminAuth_ShouldReturnCreated()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var name = $"TestBrand-{Guid.NewGuid():N}";
        var command = new CreateBrandCommand(name, name.ToLower(), "Short desc", "Long desc", null, null, 10);

        var response = await _client.PostAsJsonAsync(BrandsBase, command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var brand = await response.Content.ReadFromJsonAsync<BrandDto>();
        brand.Should().NotBeNull();
        brand!.Name.Should().Be(name);
        brand.Slug.Should().Be(name.ToLower());
        brand.ShortDescription.Should().Be("Short desc");

        ClearAuthHeader();
    }

    [Fact]
    public async Task UpdateBrand_WithAdminAuth_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var name = $"UpdBrand-{Guid.NewGuid():N}";
        var createCmd = new CreateBrandCommand(name, name.ToLower(), null, null, null, null, 0);
        var createResponse = await _client.PostAsJsonAsync(BrandsBase, createCmd);
        var created = await createResponse.Content.ReadFromJsonAsync<BrandDto>();

        var updateCmd = new UpdateBrandCommand(created!.Id, "Updated-" + name, "New short", "New long", null, null, 20);
        var response = await _client.PutAsJsonAsync($"{BrandsBase}/{created.Id}", updateCmd);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<BrandDto>();
        updated!.Name.Should().Be("Updated-" + name);
        updated.ShortDescription.Should().Be("New short");

        ClearAuthHeader();
    }

    [Fact]
    public async Task DeleteBrand_WithAdminAuth_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var name = $"DelBrand-{Guid.NewGuid():N}";
        var createCmd = new CreateBrandCommand(name, name.ToLower(), null, null, null, null, 0);
        var createResponse = await _client.PostAsJsonAsync(BrandsBase, createCmd);
        var created = await createResponse.Content.ReadFromJsonAsync<BrandDto>();

        var response = await _client.DeleteAsync($"{BrandsBase}/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"{BrandsBase}/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        ClearAuthHeader();
    }

    // ─────────────────────────────────────────────
    // Products
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetProductsList_ShouldReturnPaginatedList()
    {
        var response = await _client.GetAsync(ProductsBase);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ProductDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ShouldReturnUnauthorized()
    {
        var command = new CreateProductCommand(
            "Test Product", "test-product", "TST-001", null, null, null,
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Simple", false, null);

        var response = await _client.PostAsJsonAsync(ProductsBase, command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithAdminAuth_ShouldReturnCreated()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var allCats = await (await _client.GetAsync(CategoriesBase)).Content.ReadFromJsonAsync<PaginatedList<CategoryDto>>();
        var allBrands = await (await _client.GetAsync(BrandsBase)).Content.ReadFromJsonAsync<PaginatedList<BrandDto>>();
        var allTaxCodes = await (await _client.GetAsync(TaxCodesBase)).Content.ReadFromJsonAsync<List<ProductTaxCodeDto>>();

        var categoryId = allCats!.Items.First().Id;
        var brandId = allBrands!.Items.First().Id;
        var taxCodeId = allTaxCodes!.First().Id;

        var name = $"TestProd-{Guid.NewGuid():N}";
        var command = new CreateProductCommand(
            name, name.ToLower(), $"SKU-{Guid.NewGuid():N}", "Details", "Short", "Long",
            categoryId, brandId, taxCodeId,
            "Simple", true, 5);

        var response = await _client.PostAsJsonAsync(ProductsBase, command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.Name.Should().Be(name);
        product.Slug.Should().Be(name.ToLower());
        product.ProductType.Should().Be("Simple");
        product.CategoryId.Should().Be(categoryId);
        product.BrandId.Should().Be(brandId);

        ClearAuthHeader();
    }

    [Fact]
    public async Task GetProductById_WithExistingId_ShouldReturnOk()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var created = await CreateSampleProductAsync();

        var response = await _client.GetAsync($"{ProductsBase}/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        product.Should().NotBeNull();
        product!.Id.Should().Be(created.Id);
        product.Name.Should().Be(created.Name);

        ClearAuthHeader();
    }

    [Fact]
    public async Task GetProductBySlug_WithExistingSlug_ShouldReturnOk()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var created = await CreateSampleProductAsync();

        var response = await _client.GetAsync($"{ProductsBase}/slug/{created.Slug}");

        var body = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Xunit.Sdk.XunitException($"Expected 200 but got {(int)response.StatusCode}. Body: {body}");
        }
        var product = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        product.Should().NotBeNull();
        product!.Slug.Should().Be(created.Slug);

        ClearAuthHeader();
    }

    [Fact]
    public async Task GetProductById_WithNonExistentId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"{ProductsBase}/{Guid.NewGuid()}");

        var body = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            throw new Xunit.Sdk.XunitException($"Expected 404 but got {(int)response.StatusCode}. Body: {body}");
        }
    }

    [Fact]
    public async Task GetProductsByBrand_ShouldReturnMatchingProducts()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var allBrands = await (await _client.GetAsync(BrandsBase)).Content.ReadFromJsonAsync<PaginatedList<BrandDto>>();
        var brandId = allBrands!.Items.First().Id;

        await CreateSampleProductAsync(brandId: brandId);

        var response = await _client.GetAsync($"{ProductsBase}/brand/{brandId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ProductDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(p => p.BrandId == brandId).Should().BeTrue();

        ClearAuthHeader();
    }

    [Fact]
    public async Task GetProductsByCategory_ShouldReturnMatchingProducts()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var allCats = await (await _client.GetAsync(CategoriesBase)).Content.ReadFromJsonAsync<PaginatedList<CategoryDto>>();
        var categoryId = allCats!.Items.First().Id;

        await CreateSampleProductAsync(categoryId: categoryId);

        var response = await _client.GetAsync($"{ProductsBase}/category/{categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ProductDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(p => p.CategoryId == categoryId).Should().BeTrue();

        ClearAuthHeader();
    }

    [Fact]
    public async Task UpdateProduct_WithAdminAuth_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var created = await CreateSampleProductAsync();

        var updateCmd = new UpdateProductCommand(
            created.Id, "Updated-" + created.Name, created.Slug, "Updated details",
            "Updated short", "Updated long", created.CategoryId, created.BrandId,
            created.TaxCodeId, created.ProductType, false, 10, null, null, null);

        var response = await _client.PutAsJsonAsync($"{ProductsBase}/{created.Id}", updateCmd);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        updated!.Name.Should().Be("Updated-" + created.Name);
        updated.ShortDescription.Should().Be("Updated short");

        ClearAuthHeader();
    }

    [Fact]
    public async Task DeleteProduct_WithAdminAuth_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var created = await CreateSampleProductAsync();

        var response = await _client.DeleteAsync($"{ProductsBase}/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"{ProductsBase}/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        ClearAuthHeader();
    }

    [Fact]
    public async Task FullCatalogFlow_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        // Create category
        var catName = $"FlowCat-{Guid.NewGuid():N}";
        var catCmd = new CreateCategoryCommand(catName, catName.ToLower(), null, null, null, 0);
        var catResponse = await _client.PostAsJsonAsync(CategoriesBase, catCmd);
        catResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var category = await catResponse.Content.ReadFromJsonAsync<CategoryDto>();

        // Create brand
        var brandName = $"FlowBrand-{Guid.NewGuid():N}";
        var brandCmd = new CreateBrandCommand(brandName, brandName.ToLower(), null, null, null, null, 0);
        var brandResponse = await _client.PostAsJsonAsync(BrandsBase, brandCmd);
        brandResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var brand = await brandResponse.Content.ReadFromJsonAsync<BrandDto>();

        // Get tax code
        var taxCodes = await (await _client.GetAsync(TaxCodesBase)).Content.ReadFromJsonAsync<List<ProductTaxCodeDto>>();

        // Create product
        var prodName = $"FlowProd-{Guid.NewGuid():N}";
        var prodCmd = new CreateProductCommand(
            prodName, prodName.ToLower(), $"SKU-{Guid.NewGuid():N}",
            null, null, null,
            category!.Id, brand!.Id, taxCodes!.First().Id,
            "Simple", false, null);
        var prodResponse = await _client.PostAsJsonAsync(ProductsBase, prodCmd);
        prodResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await prodResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Get product by id
        var getResponse = await _client.GetAsync($"{ProductsBase}/{product!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get product by slug
        var slugResponse = await _client.GetAsync($"{ProductsBase}/slug/{product.Slug}");
        slugResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get products by brand
        var byBrandResponse = await _client.GetAsync($"{ProductsBase}/brand/{brand.Id}");
        byBrandResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get products by category
        var byCatResponse = await _client.GetAsync($"{ProductsBase}/category/{category.Id}");
        byCatResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Update product
        var updateCmd = new UpdateProductCommand(
            product.Id, "Updated-" + prodName, product.Slug, null, null, null,
            product.CategoryId, product.BrandId, product.TaxCodeId,
            product.ProductType, false, null, null, null, null);
        var updateResponse = await _client.PutAsJsonAsync($"{ProductsBase}/{product.Id}", updateCmd);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Delete product
        var deleteResponse = await _client.DeleteAsync($"{ProductsBase}/{product.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        ClearAuthHeader();
    }

    // ─────────────────────────────────────────────
    // Variants
    // ─────────────────────────────────────────────

    [Fact]
    public async Task AddVariant_ToSimpleProduct_ShouldReturnCreated()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var product = await CreateSampleProductAsync();

        var sku = "SKU-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        var cmd = new AddProductVariantCommand(product.Id, $"Variant-{Guid.NewGuid():N}", sku);

        var response = await _client.PostAsJsonAsync($"{ProductsBase}/{product.Id}/variants", cmd);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected 201 but got {(int)response.StatusCode}. Body: {body}");
        }
        var variant = await response.Content.ReadFromJsonAsync<VariantDto>();
        variant.Should().NotBeNull();
        variant!.ProductId.Should().Be(product.Id);
        variant.Name.Should().Be(cmd.Name);
        variant.Sku.Should().Be(cmd.Sku);
        variant.IsActive.Should().BeTrue();

        ClearAuthHeader();
    }

    [Fact]
    public async Task AddVariant_ThenVerifyViaGet_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var product = await CreateSampleProductAsync();

        var variantName = $"Variant-{Guid.NewGuid():N}";
        var variantSku = $"SKU-{Guid.NewGuid():N}";
        var addCmd = new AddProductVariantCommand(product.Id, variantName, variantSku);
        var addResponse = await _client.PostAsJsonAsync($"{ProductsBase}/{product.Id}/variants", addCmd);
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await _client.GetAsync($"{ProductsBase}/{product.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthHeader();
    }

    [Fact]
    public async Task AddMultipleVariants_ToSimpleProduct_ShouldSucceed()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var product = await CreateSampleProductAsync();

        for (int i = 0; i < 3; i++)
        {
            var cmd = new AddProductVariantCommand(product.Id, $"Var{i}-{Guid.NewGuid():N}", $"SKU-{Guid.NewGuid():N}");
            var response = await _client.PostAsJsonAsync($"{ProductsBase}/{product.Id}/variants", cmd);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        ClearAuthHeader();
    }

    [Fact]
    public async Task AddVariant_ToNonExistentProduct_ShouldReturnNotFound()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var productId = Guid.NewGuid();
        var cmd = new AddProductVariantCommand(productId, "Test", "TST-001");
        var response = await _client.PostAsJsonAsync($"{ProductsBase}/{productId}/variants", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        ClearAuthHeader();
    }

    [Fact]
    public async Task AddVariant_WithoutAuth_ShouldReturnUnauthorized()
    {
        var cmd = new AddProductVariantCommand(Guid.NewGuid(), "Test", "TST-001");
        var response = await _client.PostAsJsonAsync($"{ProductsBase}/{Guid.NewGuid()}/variants", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────

    private async Task<ProductDto> CreateSampleProductAsync(Guid? categoryId = null, Guid? brandId = null)
    {
        var allCats = await (await _client.GetAsync(CategoriesBase)).Content.ReadFromJsonAsync<PaginatedList<CategoryDto>>();
        var allBrands = await (await _client.GetAsync(BrandsBase)).Content.ReadFromJsonAsync<PaginatedList<BrandDto>>();
        var allTaxCodes = await (await _client.GetAsync(TaxCodesBase)).Content.ReadFromJsonAsync<List<ProductTaxCodeDto>>();

        var cmd = new CreateProductCommand(
            $"Sample-{Guid.NewGuid():N}",
            $"sample-{Guid.NewGuid():N}",
            $"SKU-{Guid.NewGuid():N}",
            null, null, null,
            categoryId ?? allCats!.Items.First().Id,
            brandId ?? allBrands!.Items.First().Id,
            allTaxCodes!.First().Id,
            "Simple", false, null);

        var response = await _client.PostAsJsonAsync(ProductsBase, cmd);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }
}
