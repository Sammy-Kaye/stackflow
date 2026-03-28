// BaseApiController is the base class for all StackFlow API controllers.
// It provides:
//   - The [ApiController] and [Route] attributes that every controller needs
//   - HandleResult<T>() — maps a Result<T> from a handler to the correct HTTP status code
//
// Every controller in StackFlow extends this class. Controllers never contain business logic.
// One line per endpoint: return HandleResult(await Mediator.Send(command));
//
// HandleResult mapping:
//   Result.Success(value)  → 200 OK  with the value as the response body
//   Result.Fail(message)   → 400 Bad Request with { "error": message }
//
// The Mediator property is intentionally left as a comment placeholder here.
// It will be injected in Feature 5 (Custom Mediator + Pipeline) once the mediator exists.
// Controllers written in Features 2–4 will inject their dependencies directly until then.

using Microsoft.AspNetCore.Mvc;
using StackFlow.Application.Common;

namespace StackFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    // Mediator is injected here once the custom mediator is built (Feature 5).
    // Until then, individual controllers inject their own dependencies.

    /// <summary>
    /// Maps a Result carrying a value to the appropriate HTTP response.
    /// IsSuccess = true  → 200 OK  with the value serialised as the response body.
    /// IsSuccess = false → 400 Bad Request with { "error": "..." }.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Maps a non-generic Result (commands that return no value) to the appropriate HTTP response.
    /// IsSuccess = true  → 200 OK  with an empty body.
    /// IsSuccess = false → 400 Bad Request with { "error": "..." }.
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok();

        return BadRequest(new { error = result.Error });
    }
}
