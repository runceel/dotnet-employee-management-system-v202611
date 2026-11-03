using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EmployeeManagement.IntegrationTests;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> application)
    {
        _client = application.CreateClient();
    }

    [Fact]
    public async Task HealthEndpointReturnsHealthyStatus()
    {
        using var response = await _client.GetAsync("/health");
        var health = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", health, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApiHealthEndpointReturnsHealthyStatusAsJson()
    {
        var health = await _client.GetFromJsonAsync<ApiHealthResponse>("/api/health");

        Assert.Equal("Healthy", health?.Status);
    }

    private sealed record ApiHealthResponse(string Status);
}
