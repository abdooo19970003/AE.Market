using System.Net;
using FluentAssertions;

namespace AE.Market.Integration.Tests.Analytics;

[Collection("Integration tests")]
public sealed class HealthCheckTests(IntegrationTestWebAppFactory factory)
{
    private readonly HttpClient _client = factory.HttpClient;

    [Fact]
    public async Task Health_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReady_ReturnsSuccessOrServiceUnavailable()
    {
        var response = await _client.GetAsync("/health/ready");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Health_WithoutAuth_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
