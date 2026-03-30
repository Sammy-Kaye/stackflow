// PingCommandHandler — smoke-test handler for PingCommand.
//
// Returns Result.Ok(new { message = "pong" }) unconditionally. Its only purpose is to confirm that:
//   1. The mediator resolved this handler from DI via assembly scanning
//   2. The pipeline (ValidationBehavior → LoggingBehavior) executed before reaching here
//   3. The result propagated back through BaseApiController.HandleResult to the HTTP response
//      as the JSON shape { "message": "pong" } required by the API contract
//
// This handler is intentionally trivial. It exercises the wiring, not any business logic.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.Application.Features.Ping;

/// <summary>
/// Handles PingCommand. Returns { "message": "pong" } to confirm the mediator pipeline is operational.
/// </summary>
public sealed class PingCommandHandler : IRequestHandler<PingCommand, Result<object>>
{
    public Task<Result<object>> Handle(PingCommand request, CancellationToken ct)
        => Task.FromResult(Result.Ok<object>(new { message = "pong" }));
}
