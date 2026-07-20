using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AE.Market.Application.Features.Auth.Commands.Login;
using AE.Market.Application.Features.Auth.DTOs;
using FluentAssertions;

namespace AE.Market.Integration.Tests.Analytics;

[Collection("Integration tests")]
public sealed class AdminStatsTests(IntegrationTestWebAppFactory factory)
{
    private readonly HttpClient _client = factory.HttpClient;
    private const string AuthBase = "api/auth";
    private const string AdminBase = "api/admin";

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

    [Fact]
    public async Task GetStats_WithValidToken_ReturnsOkAndStats()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var response = await _client.GetAsync($"{AdminBase}/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("totalProducts");
        content.Should().Contain("activeStock");
        content.Should().Contain("averagePrice");

        ClearAuthHeader();
    }

    [Fact]
    public async Task GetTopProducts_WithValidToken_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var response = await _client.GetAsync($"{AdminBase}/stats/top-products?days=30&top=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ClearAuthHeader();
    }

    [Fact]
    public async Task GetTopSearches_WithValidToken_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var response = await _client.GetAsync($"{AdminBase}/stats/top-searches?days=30&top=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ClearAuthHeader();
    }

    [Fact]
    public async Task GetStats_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthHeader();

        var response = await _client.GetAsync($"{AdminBase}/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTopProducts_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthHeader();

        var response = await _client.GetAsync($"{AdminBase}/stats/top-products?days=30&top=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTopSearches_WithoutAuth_ReturnsUnauthorized()
    {
        ClearAuthHeader();

        var response = await _client.GetAsync($"{AdminBase}/stats/top-searches?days=30&top=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
