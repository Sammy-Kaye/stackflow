# Feature Brief: Custom Mediator + Pipeline
Phase: 1
Feature: #5
Status: Ready for implementation

---

## What this feature does (plain English)

This feature installs the CQRS backbone that every future handler in StackFlow depends on.
Rather than using MediatR, a hand-rolled mediator is built so that the dispatch mechanism
is fully understood, fully readable, and carries no third-party black-box behaviour.
Once this feature is complete, every command and query in the system flows through a
single `Mediator.Send()` call, automatically passing through validation and logging
pipeline steps before reaching the handler.

## Scope — what IS in this brief

- `IRequest<TResponse>`, `ICommand<TResponse>`, `IQuery<TResponse>` marker interfaces
- `IRequestHandler<TRequest, TResponse>` handler contract interface
- `IPipelineBehavior<TRequest, TResponse>` pipeline middleware interface
- Concrete `Mediator` class — resolves handler from DI, wraps in pipeline behaviors in
  registration order, executes the chain
- `ValidationBehavior<TRequest, TResponse>` — resolves all `IValidator<TRequest>` instances
  from DI, runs all validators, short-circuits with `Result.Fail(validationErrors)` if any fail
- `LoggingBehavior<TRequest, TResponse>` — logs request type name and elapsed milliseconds
  via `ILogger`; logs at `Information` level on success, `Warning` level on failure
- Assembly scanning DI registration — all `IRequestHandler<,>` implementations in
  `StackFlow.Application` are registered automatically; no manual handler wiring
- `BaseApiController` updated to inject `Mediator` as a protected property, replacing the
  placeholder comment left in Feature 1
- `AddApplication()` extension method added to `StackFlow.Application` to hold Application-layer
  DI registrations; called from `Program.cs`
- `PingCommand` smoke-test command — a minimal command + handler used only to verify the
  full pipeline wires correctly; can be removed after Feature 8 but kept as a health check
  aid in development
- `GET /api/ping` controller endpoint that dispatches `PingCommand` via `Mediator.Send()`
  and returns the string result — used to verify the pipeline in development
- `HandleResult` mapping on `BaseApiController` expanded to cover "not found" and "forbidden"
  error strings, matching the PHASE1-BUILD-PLAN specification:
  - `IsSuccess` + value → `200 OK` with value
  - `IsFailure` + error containing "not found" (case-insensitive) → `404 NotFound` with `{ error }`
  - `IsFailure` + error containing "forbidden" (case-insensitive) → `403 Forbidden`
  - All other failures → `400 BadRequest` with `{ error }`

## Scope — what is NOT in this brief

- No actual business commands or queries — those belong to the features that use them
  (Feature 8 onwards)
- No FluentValidation validators for real entities — validators are added per-feature
- No RabbitMQ or SignalR integration — messaging is Phase 2
- No changes to domain entities, EF Core configuration, or repositories
- No frontend work — this feature has no API contract that the frontend consumes directly
- The `PingCommand` and `GET /api/ping` endpoint are a development convenience only;
  they are not part of the application's business surface

## Domain entities involved

None. This feature operates entirely within the `StackFlow.Application` and
`StackFlow.Api` layers. No domain entity mutations occur.

## API Contract

This feature is backend-infrastructure only. There is one developer-facing endpoint
created as a smoke test, not consumed by the frontend in any feature.

#### GET /api/ping
Auth: JWT required

Request body: none

Response 200:
{
  message: string
}

Response 401: { error: "Unauthorised" }

---

No other endpoints are added or modified by this feature.

## Frontend routes and views

None. This feature has no frontend component.

## RabbitMQ events (if any)

None for this feature.

## SignalR events (if any)

None for this feature.

## Audit requirements

None. This feature contains no business state mutations. The `LoggingBehavior` provides
structural logging of every request/response cycle, but writes no audit trail entries.

## Acceptance criteria

1. Given the application starts, when `dotnet build` runs against the solution, then it
   completes with zero warnings and zero errors.
2. Given the application is running, when `GET /api/ping` is called with a valid dev JWT,
   then the response is `200 OK` with `{ "message": "pong" }` (or equivalent confirmation
   string).
3. Given a `PingCommand` is dispatched via `Mediator.Send()`, when the call completes,
   then the `LoggingBehavior` has written an `Information`-level log entry containing the
   request type name and elapsed milliseconds.
4. Given a command is dispatched that has a registered `IValidator<TRequest>`, when the
   validator returns one or more failures, then `ValidationBehavior` returns
   `Result.Fail(...)` and the handler's `Handle` method is never called.
5. Given a command is dispatched that has a registered `IValidator<TRequest>`, when the
   validator passes, then execution proceeds through `LoggingBehavior` and reaches the
   handler.
6. Given a handler is added to `StackFlow.Application` implementing
   `IRequestHandler<TRequest, TResponse>`, when the application starts, then that handler
   is resolvable from the DI container without any manual registration.
7. Given a controller calls `HandleResult` with a failed result whose error contains
   "not found", then the HTTP response is `404 Not Found`.
8. Given a controller calls `HandleResult` with a failed result whose error contains
   "forbidden", then the HTTP response is `403 Forbidden`.
9. Given a controller calls `HandleResult` with a failed result whose error contains
   neither "not found" nor "forbidden", then the HTTP response is `400 Bad Request` with
   `{ "error": "<error message>" }`.

## Agent instructions

Backend Agent:
1. Create `StackFlow.Application/Common/Mediator/` and add the five interfaces:
   `IRequest<TResponse>`, `ICommand<TResponse>`, `IQuery<TResponse>`,
   `IRequestHandler<TRequest, TResponse>`, `IPipelineBehavior<TRequest, TResponse>`.
   Each interface in its own file. Include a brief XML doc comment explaining its role.
2. Create `StackFlow.Application/Common/Mediator/Mediator.cs` — the concrete `Mediator`
   class. It accepts `IServiceProvider` via constructor injection, resolves the concrete
   `IRequestHandler<TRequest, TResponse>` from DI, wraps it with all registered
   `IEnumerable<IPipelineBehavior<TRequest, TResponse>>` in order, and executes the chain.
3. Create `StackFlow.Application/Common/Behaviors/ValidationBehavior.cs` —
   `ValidationBehavior<TRequest, TResponse>` implementing
   `IPipelineBehavior<TRequest, TResponse>`. Accepts
   `IEnumerable<IValidator<TRequest>>` via constructor. Runs all validators; if any fail,
   returns `Result.Fail(string.Join("; ", errors))` without calling `next`. The return type
   constraint on `TResponse` must be `Result` (or `Result<T>`) so `Result.Fail` is
   accessible — use a where clause: `where TResponse : Result`.
4. Create `StackFlow.Application/Common/Behaviors/LoggingBehavior.cs` —
   `LoggingBehavior<TRequest, TResponse>` implementing
   `IPipelineBehavior<TRequest, TResponse>`. Accepts `ILogger<LoggingBehavior<TRequest,
   TResponse>>` via constructor. Records a `Stopwatch`, calls `next`, logs the request type
   name and elapsed time at `Information` on success or `Warning` on failure.
5. Create `StackFlow.Application/DependencyInjection.cs` — `AddApplication()` extension
   method on `IServiceCollection`. Registers: `Mediator` as scoped; all
   `IRequestHandler<,>` implementations from the `StackFlow.Application` assembly via
   open-generic assembly scanning; `ValidationBehavior` and `LoggingBehavior` as scoped
   `IPipelineBehavior<,>` in that order (validation first).
6. Update `StackFlow.Application/StackFlow.Application.csproj` — add
   `Microsoft.Extensions.DependencyInjection.Abstractions` and
   `Microsoft.Extensions.Logging.Abstractions` package references (version `10.*`) so the
   Application layer can reference `IServiceProvider`, `IServiceCollection`, and `ILogger`
   without depending on the Infrastructure or API layers.
7. Add `PingCommand` to `StackFlow.Application/Features/Ping/` — a minimal
   `ICommand<Result<string>>` and a `PingCommandHandler` that returns
   `Result.Ok<string>("pong")`.
8. Update `StackFlow.Api/Controllers/BaseApiController.cs` — inject `Mediator` as a
   protected property via constructor or `[FromServices]` property injection (whichever
   is cleaner). Expand `HandleResult` to add the "not found" → 404 and "forbidden" → 403
   mappings described in the scope section above, in addition to the existing 200 / 400
   mappings.
9. Add `StackFlow.Api/Controllers/PingController.cs` — a minimal controller extending
   `BaseApiController` with a single `GET /api/ping` endpoint that dispatches
   `PingCommand` and returns `HandleResult(...)`. Decorate with `[Authorize]`.
10. Update `Program.cs` — call `builder.Services.AddApplication()` alongside the existing
    `AddInfrastructure()` call.
11. Run `dotnet build` from `web-api/` and confirm zero warnings. Fix any that appear
    before declaring done.

Frontend Agent: No work for this feature.

Handoff point: Not applicable — this feature is backend-only. When the Backend Agent
declares done, Samuel can call `GET /api/ping` with a dev JWT to confirm the pipeline
is live, then move to Feature 6 (App Shell + Routing).
