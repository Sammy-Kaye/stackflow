// HealthEndpointTests — integration test for GET /health.
//
// Uses WebApplicationFactory<Program> to spin up the full ASP.NET Core pipeline
// in-process. No external services (database, RabbitMQ) are required — the health
// endpoint has no dependencies beyond the running process.
//
// Program is accessible here because Program.cs declares `public partial class Program { }`.
//
// Covered behaviours:
//   GET /health → HTTP 200 with body { "status": "healthy" }

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StackFlow.IntegrationTests.Api;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        // CreateClient() starts the in-process test host using the real Program.cs pipeline.
        // No services are replaced here — the health endpoint has no external dependencies
        // to stub out.
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_Health_Returns200_WithHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert — status code
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert — response body shape and value
        // ReadFromJsonAsync deserialises using the same camelCase policy configured
        // in Program.cs, so "status" maps to the Status property correctly.
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("healthy", body!.Status);
    }

    // Local record matching the API contract shape: { "status": string }
    // Defined here so this test file is self-contained.
    private record HealthResponse(string Status);
}
