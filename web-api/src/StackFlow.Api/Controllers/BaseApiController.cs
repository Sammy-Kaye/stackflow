// BaseApiController is the base class for all StackFlow API controllers.
// It provides:
//   - The [ApiController] and [Route] attributes that every controller needs
//   - A protected Mediator property injected via constructor — used by every endpoint
//   - HandleResult<T>() and HandleResult() — maps Results from handlers to HTTP status codes
//
// Every controller in StackFlow extends this class. Controllers never contain business logic.
// One line per endpoint: return HandleResult(await Mediator.Send(command));
//
// HandleResult mapping:
//   Result.Success(value)                    → 200 OK  with the value as the response body
//   Result.Success() (non-generic)           → 200 OK  with an empty body
//   Result.Fail("...not found...")           → 404 Not Found  with { "error": "..." }
//   Result.Fail("...forbidden...")           → 403 Forbidden  (no body)
//   Result.Fail("any other message")         → 400 Bad Request with { "error": "..." }
//
// The "not found" and "forbidden" checks are case-insensitive substring matches.

using Microsoft.AspNetCore.Mvc;
using StackFlow.Application.Common;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    // Mediator is injected via the constructor of each concrete controller.
    // Concrete controllers must declare a constructor that accepts Mediator and
    // passes it to base(mediator).
    protected Mediator Mediator { get; }

    protected BaseApiController(Mediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// Maps a Result carrying a value to the appropriate HTTP response.
    /// IsSuccess = true              → 200 OK  with the value serialised as the response body.
    /// IsFailure + "not found"       → 404 Not Found with { "error": "..." }.
    /// IsFailure + "forbidden"       → 403 Forbidden (no body).
    /// IsFailure (anything else)     → 400 Bad Request with { "error": "..." }.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return MapFailure(result.Error);
    }

    /// <summary>
    /// Maps a non-generic Result (commands that return no value) to the appropriate HTTP response.
    /// IsSuccess = true              → 200 OK  with an empty body.
    /// IsFailure + "not found"       → 404 Not Found with { "error": "..." }.
    /// IsFailure + "forbidden"       → 403 Forbidden (no body).
    /// IsFailure (anything else)     → 400 Bad Request with { "error": "..." }.
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok();

        return MapFailure(result.Error);
    }

    // Maps a failure error string to the correct HTTP status code.
    // Extracted so both HandleResult overloads share the same mapping logic.
    private IActionResult MapFailure(string error)
    {
        if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { error });

        if (error.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
            return StatusCode(StatusCodes.Status403Forbidden);

        return BadRequest(new { error });
    }
}
