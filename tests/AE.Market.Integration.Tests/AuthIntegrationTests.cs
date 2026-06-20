using System.Net;
using System.Net.Http.Json;
using AE.Market.Application.Features.Auth.Commands.Register;
using FluentAssertions;

namespace AE.Market.Integration.Tests;

[Collection("Integration tests")]
public sealed class AuthIntegrationTests(IntegrationTestWebAppFactory factory)
{
    private readonly HttpClient _client = factory.HttpClient;
    private const string BasePath = "api/auth";

    private static void AssertValidRefreshToken(string refreshToken)
    {
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().Contain(".");

        var parts = refreshToken.Split('.');
        parts.Should().HaveCount(2);
        parts[0].Should().NotBeNullOrEmpty();
        parts[1].Should().NotBeNullOrEmpty();

        var decoded = parts[0]
            .Replace('-', '+')
            .Replace('_', '/');
        decoded = decoded.PadRight(decoded.Length + (4 - decoded.Length % 4) % 4, '=');
        var bytes = Convert.FromBase64String(decoded);
        bytes.Should().HaveCount(16);
        var guid = new Guid(bytes);
        guid.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_ShouldCreateUser_AndReturnTokens()
    {
        var command = new RegisterCommand("register1@example.com", "Password123");

        var response = await _client.PostAsJsonAsync($"{BasePath}/register", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var tokens = await response.Content.ReadFromJsonAsync<TokensResponseDto>();
        tokens.Should().NotBeNull();
        tokens!.AccessToken.Should().NotBeNullOrEmpty();
        AssertValidRefreshToken(tokens.RefreshToken);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var command = new RegisterCommand("not-an-email", "Password123");

        var response = await _client.PostAsJsonAsync($"{BasePath}/register", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        var command = new RegisterCommand("weak@example.com", "123");

        var response = await _client.PostAsJsonAsync($"{BasePath}/register", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldReturnConflict()
    {
        var email = "duplicate@example.com";
        var command = new RegisterCommand(email, "Password123");

        var first = await _client.PostAsJsonAsync($"{BasePath}/register", command);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync($"{BasePath}/register", command);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        var password = "Pass1234";

        await _client.PostAsJsonAsync($"{BasePath}/register", new RegisterCommand(email, password));

        var loginResponse = await _client.PostAsJsonAsync($"{BasePath}/login",
            new { Email = email, Password = password });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokensResponseDto>();
        tokens.Should().NotBeNull();
        tokens!.AccessToken.Should().NotBeNullOrEmpty();
        AssertValidRefreshToken(tokens.RefreshToken);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        var email = $"wrongpass-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync($"{BasePath}/register", new RegisterCommand(email, "Correct123"));

        var loginResponse = await _client.PostAsJsonAsync($"{BasePath}/login",
            new { Email = email, Password = "WrongPass1" });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturnUnauthorized()
    {
        var loginResponse = await _client.PostAsJsonAsync($"{BasePath}/login",
            new { Email = "nonexistent@example.com", Password = "SomePass123" });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ShouldReturnOk()
    {
        var email = $"me-{Guid.NewGuid():N}@example.com";
        var password = "MeTest123";

        var registerResponse = await _client.PostAsJsonAsync($"{BasePath}/register",
            new RegisterCommand(email, password));
        var registerTokens = await registerResponse.Content.ReadFromJsonAsync<TokensResponseDto>();

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BasePath}/me");
        request.Headers.Authorization = new("Bearer", registerTokens!.AccessToken);
        var meResponse = await _client.SendAsync(request);

        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMe_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync($"{BasePath}/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        var email = $"refresh-{Guid.NewGuid():N}@example.com";
        var password = "Refres123";

        var registerResponse = await _client.PostAsJsonAsync($"{BasePath}/register",
            new RegisterCommand(email, password));
        var registerTokens = await registerResponse.Content.ReadFromJsonAsync<TokensResponseDto>();

        AssertValidRefreshToken(registerTokens!.RefreshToken);

        var refreshResponse = await _client.PostAsJsonAsync($"{BasePath}/refresh",
            new { OldToken = registerTokens.RefreshToken });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await refreshResponse.Content.ReadFromJsonAsync<TokensResponseDto>();
        newTokens.Should().NotBeNull();
        newTokens!.AccessToken.Should().NotBeNullOrEmpty();
        AssertValidRefreshToken(newTokens.RefreshToken);
        newTokens.RefreshToken.Should().NotBe(registerTokens.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ShouldReturnUnauthorized()
    {
        var email = $"reused-{Guid.NewGuid():N}@example.com";
        var password = "Reuse1234";

        var registerResponse = await _client.PostAsJsonAsync($"{BasePath}/register",
            new RegisterCommand(email, password));
        var registerTokens = await registerResponse.Content.ReadFromJsonAsync<TokensResponseDto>();

        var firstRefresh = await _client.PostAsJsonAsync($"{BasePath}/refresh",
            new { OldToken = registerTokens!.RefreshToken });
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondRefresh = await _client.PostAsJsonAsync($"{BasePath}/refresh",
            new { OldToken = registerTokens.RefreshToken });
        secondRefresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithMalformedToken_ShouldReturnNotFound()
    {
        var response = await _client.PostAsJsonAsync($"{BasePath}/refresh",
            new { OldToken = "not-a-valid-token-format" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Refresh_WithNonExistentTokenId_ShouldReturnNotFound()
    {
        var fakeTokenId = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var response = await _client.PostAsJsonAsync($"{BasePath}/refresh",
            new { OldToken = $"{fakeTokenId}.someRandomBase64Data" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Logout_ShouldSucceed_AndRevokeTokens()
    {
        var email = $"logout-{Guid.NewGuid():N}@example.com";
        var password = "Logout123";

        var registerResponse = await _client.PostAsJsonAsync($"{BasePath}/register",
            new RegisterCommand(email, password));
        var tokens = await registerResponse.Content.ReadFromJsonAsync<TokensResponseDto>();

        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, $"{BasePath}/logout");
        logoutRequest.Headers.Authorization = new("Bearer", tokens!.AccessToken);
        var logoutResponse = await _client.SendAsync(logoutRequest);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await _client.PostAsJsonAsync($"{BasePath}/refresh",
            new { OldToken = tokens.RefreshToken });
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullAuthFlow_ShouldSucceed()
    {
        var email = $"flow-{Guid.NewGuid():N}@example.com";
        var password = "Flow1234";

        var registerResponse = await _client.PostAsJsonAsync($"{BasePath}/register",
            new RegisterCommand(email, password));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tokens = await registerResponse.Content.ReadFromJsonAsync<TokensResponseDto>();
        tokens.Should().NotBeNull();

        var meRequest = new HttpRequestMessage(HttpMethod.Get, $"{BasePath}/me");
        meRequest.Headers.Authorization = new("Bearer", tokens!.AccessToken);
        var meResponse = await _client.SendAsync(meRequest);
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await _client.PostAsJsonAsync($"{BasePath}/login",
            new { Email = email, Password = password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginTokens = await loginResponse.Content.ReadFromJsonAsync<TokensResponseDto>();

        var refreshResponse = await _client.PostAsJsonAsync($"{BasePath}/refresh",
            new { OldToken = loginTokens!.RefreshToken });
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await refreshResponse.Content.ReadFromJsonAsync<TokensResponseDto>();

        var me2Request = new HttpRequestMessage(HttpMethod.Get, $"{BasePath}/me");
        me2Request.Headers.Authorization = new("Bearer", newTokens!.AccessToken);
        var me2Response = await _client.SendAsync(me2Request);
        me2Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, $"{BasePath}/logout");
        logoutRequest.Headers.Authorization = new("Bearer", newTokens.AccessToken);
        var logoutResponse = await _client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsync($"{BasePath}/logout", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record TokensResponseDto(string AccessToken, string RefreshToken, Guid? UserId);
}