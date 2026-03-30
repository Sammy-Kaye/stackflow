# Custom Mediator + Pipeline

> Last updated: 2026-03-30
> Phase: 1
> Status: ✅ Complete — PR approved

---

## What it does

This feature installs the CQRS backbone that all future StackFlow handlers depend on. Instead of using MediatR (a third-party library), a hand-rolled mediator was built so every dispatch path is fully readable and debuggable. Every command and query in the system now flows through a single `Mediator.Send()` call that automatically runs validation and logging before reaching the handler. This is the infrastructure layer that makes features like Workflow CRUD (Feature 8) possible.

---

## How it works

**Request dispatch flow:**
When a controller calls `Mediator.Send(new MyCommand(...))`, the mediator:
1. Resolves the concrete `IRequestHandler<MyCommand, Result<T>>` from the DI container
2. Wraps it with all registered `IPipelineBehavior<MyCommand, Result<T>>` instances in order
3. Executes the pipeline chain:
   - **ValidationBehavior** runs first — it resolves all `IValidator<MyCommand>` instances from DI, runs them in parallel, and returns `Result.Fail(...)` if any validation fails. If validation passes, it calls `next()`.
   - **LoggingBehavior** runs second — it starts a stopwatch, calls `next()` to invoke the handler, then logs the request type name and elapsed milliseconds at `Information` level on success or `Warning` level on failure.
   - The **handler** (your business logic) receives the request only after validation has passed. It applies business rules, calls repositories, and returns `Result.Ok(value)` or `Result.Fail(error)`.
4. The result bubbles back through the pipeline and returns to the controller
5. The controller calls `HandleResult()` on the `BaseApiController`, which maps the `Result<T>` to the correct HTTP status code (200, 400, 403, or 404)

**Why hand-rolled instead of MediatR?**
Every agent and future developer working on StackFlow needs to understand how requests flow through the system. A black-box library obscures that. This custom mediator is ~80 lines of fully-commented code that any developer can read in 5 minutes and understand completely.

---

## Key files

| File | Purpose |
|---|---|
| `web-api/src/StackFlow.Application/Common/Mediator/IRequest.cs` | Marker interface for all requests |
| `web-api/src/StackFlow.Application/Common/Mediator/ICommand.cs` | Marker interface for write operations |
| `web-api/src/StackFlow.Application/Common/Mediator/IQuery.cs` | Marker interface for read operations |
| `web-api/src/StackFlow.Application/Common/Mediator/IRequestHandler.cs` | Handler contract — one per request type |
| `web-api/src/StackFlow.Application/Common/Mediator/IPipelineBehavior.cs` | Middleware contract for cross-cutting concerns |
| `web-api/src/StackFlow.Application/Common/Mediator/Mediator.cs` | The dispatch hub — resolves handlers and wraps with behaviors |
| `web-api/src/StackFlow.Application/Common/Behaviors/ValidationBehavior.cs` | Pipeline step 1 — runs FluentValidation validators |
| `web-api/src/StackFlow.Application/Common/Behaviors/LoggingBehavior.cs` | Pipeline step 2 — logs requests with timing |
| `web-api/src/StackFlow.Application/DependencyInjection.cs` | Registers mediator, handlers (via assembly scan), and behaviors |
| `web-api/src/StackFlow.Application/Features/Ping/PingCommand.cs` | Smoke-test command — minimal command for wiring verification |
| `web-api/src/StackFlow.Application/Features/Ping/PingCommandHandler.cs` | Smoke-test handler — returns `"pong"` unconditionally |
| `web-api/src/StackFlow.Api/Controllers/BaseApiController.cs` | Base controller for all API endpoints — injects Mediator, provides HandleResult |
| `web-api/src/StackFlow.Api/Controllers/PingController.cs` | Smoke-test endpoint — `GET /api/ping` dispatches PingCommand |
| `web-api/src/StackFlow.Api/Program.cs` | Calls `builder.Services.AddApplication()` to register DI |

---

## Database changes

No database changes in this feature.

---

## Events

No RabbitMQ events in this feature.

---

## Real-time (SignalR)

No SignalR events in this feature.

---

## Known limitations or caveats

**PingCommand and PingController are temporary.** They exist only to verify the pipeline wires correctly during development. They are not consumed by the frontend and can be removed after Feature 8 (Workflow CRUD), though they serve as a useful health check in production.

**Dynamic casting in Mediator.** The mediator uses `dynamic` to invoke handlers and behaviors at runtime because the exact types are resolved from the DI container and would require explicit casting. This is safe — the assembly scanning in `DependencyInjection.cs` ensures types are registered correctly — but it's worth understanding if you need to debug dispatch failures.

**ValidationBehavior uses reflection to handle Result vs Result<T>.** When validation fails, the behavior must return a `Result` or `Result<T>` depending on what the handler's type signature expects. It uses reflection to call `Result.Fail<T>()` for generic results. This is correct and well-commented, but it's a subtle piece of code.

---

## Notes: Brief vs implementation

Implementation matches the Feature Brief exactly.

---
