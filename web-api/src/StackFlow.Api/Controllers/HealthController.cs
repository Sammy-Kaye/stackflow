// HealthController exposes GET /health.
// This endpoint is public, requires no auth, and always returns HTTP 200 while the process runs.
// Its sole purpose is to confirm the API process is up and reachable.
// Docker Compose, load balancers, and monitoring tools hit this endpoint.
//
// API Contract (Feature Brief: Feature 1 — Project Scaffold):
//   GET /health
//   Response 200: { "status": "healthy" }

using Microsoft.AspNetCore.Mvc;

namespace StackFlow.Api.Controllers;

[Route("health")]
public class HealthController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new HealthResponse("healthy"));
    }
}

// Response record — matches the API contract exactly.
// Record chosen for immutability and concise syntax. No business logic here.
public record HealthResponse(string Status);
