// PingController — smoke-test endpoint for verifying the mediator pipeline end-to-end.
//
// GET /api/ping dispatches PingCommand via Mediator.Send() and returns the result.
// Used during development to confirm:
//   1. The mediator resolves PingCommandHandler from DI
//   2. The pipeline (ValidationBehavior → LoggingBehavior) executes before the handler
//   3. HandleResult maps the Result<object> to a 200 OK with { "message": "pong" } in the body
//
// This endpoint can be removed after Feature 8 but is kept as a development health check.
// Requires a valid JWT — the same dev token issued by POST /api/auth/dev-login works.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Ping;

namespace StackFlow.Api.Controllers;

[Authorize]
public class PingController : BaseApiController
{
    public PingController(Mediator mediator) : base(mediator) { }

    /// <summary>
    /// Smoke test. Dispatches PingCommand through the full mediator pipeline.
    /// Returns 200 OK with { "message": "pong" } when the pipeline is operational.
    /// Returns 401 Unauthorised when no valid JWT is provided.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Ping(CancellationToken ct)
        => HandleResult(await Mediator.Send(new PingCommand(), ct));
}
