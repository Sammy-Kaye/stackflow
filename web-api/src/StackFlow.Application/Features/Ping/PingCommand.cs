// PingCommand — smoke-test command to verify the full mediator pipeline is wired correctly.
//
// Dispatched by GET /api/ping. Returns Result.Ok("pong") if the pipeline executes end-to-end.
// Can be removed after Feature 8 but kept as a development health check aid.
//
// This command has no validator registered — so ValidationBehavior passes through immediately.
// LoggingBehavior will log: "Request PingCommand completed successfully in Xms"

using StackFlow.Application.Common;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.Application.Features.Ping;

/// <summary>
/// Smoke-test command. Returns { "message": "pong" } to confirm the mediator pipeline is operational.
/// </summary>
public record PingCommand() : ICommand<Result<object>>;
