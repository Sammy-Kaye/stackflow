# Project Scaffold API Reference

> Last updated: 2026-03-28
> Feature status: Approved — PR reviewed
> Phase: 1, Feature 1
> Related files: `web-api/src/StackFlow.Api/Controllers/HealthController.cs`, `web-api/src/StackFlow.Api/Controllers/BaseApiController.cs`, `web-api/src/StackFlow.Application/Common/Result.cs`

---

## GET /health

Returns a confirmation that the API process is running. Used by Docker Compose health
checks, load balancers, and monitoring tools.

Note: `HealthController` extends `BaseApiController` but its `[Route]` attribute is set
to `"health"` directly, not `"api/[controller]"`. The path is `/health`, not `/api/health`.

**Auth:** Public — no token required

### Request body

No request body.

### Response 200

| Field | Type | Description |
|---|---|---|
| `status` | string | Always `"healthy"` while the process is running |

### Error responses

None. While the process is running this endpoint always returns 200. If the process is
down the connection fails at the network level before any HTTP response is returned.

### Example

**Response:**
```json
{ "status": "healthy" }
```

---

## BaseApiController — HandleResult pattern

`BaseApiController` is the abstract base class that every StackFlow controller extends.
It lives at `web-api/src/StackFlow.Api/Controllers/BaseApiController.cs`.

Its primary job is the `HandleResult` method, which maps a `Result` or `Result<T>` from
a handler into the correct HTTP response. Controllers never inspect the result themselves —
they return `HandleResult(...)` directly.

There are two overloads:

**HandleResult\<T\>(Result\<T\> result)** — for queries and commands that return a value:

| Result state | HTTP response |
|---|---|
| `IsSuccess = true` | 200 OK with `result.Value` serialised as the response body |
| `IsSuccess = false` | 400 Bad Request with `{ "error": "<result.Error>" }` |

**HandleResult(Result result)** — for commands that return no value on success:

| Result state | HTTP response |
|---|---|
| `IsSuccess = true` | 200 OK with an empty body |
| `IsSuccess = false` | 400 Bad Request with `{ "error": "<result.Error>" }` |

Typical controller usage (one line per endpoint):
```csharp
return HandleResult(await Mediator.Send(command));
```

Note: The `Mediator` property is not yet injected in this scaffold feature. It will be
wired in Feature 5 (Custom Mediator + Pipeline). Until then, controllers inject their
own dependencies directly.

---

## Result\<T\> — return type for all handlers

`Result` and `Result<T>` are defined in `web-api/src/StackFlow.Application/Common/Result.cs`.
They are the only way handlers communicate success or failure. Business exceptions are
never thrown.

### Result (non-generic) — for commands that return no value

| Member | Type | Description |
|---|---|---|
| `IsSuccess` | bool | `true` if the operation succeeded |
| `Error` | string | Error message when `IsSuccess` is `false`; empty string on success |

Factory methods:

| Method | Returns | When to use |
|---|---|---|
| `Result.Ok()` | `Result` (success) | Command completed successfully with no return value |
| `Result.Fail(string error)` | `Result` (failure) | Command failed — pass a plain English error message |

### Result\<T\> (generic) — for queries and commands that return a value

Extends `Result`. Adds:

| Member | Type | Description |
|---|---|---|
| `Value` | `T` | The success value. Throws `InvalidOperationException` if accessed on a failed result |

Factory methods:

| Method | Returns | When to use |
|---|---|---|
| `Result<T>.Ok(T value)` | `Result<T>` (success) | Operation succeeded — pass the return value |
| `Result<T>.Fail(string error)` | `Result<T>` (failure) | Operation failed — pass a plain English error message |

Convenience factories on the non-generic `Result` type (both delegate to the generic):

| Method | Returns |
|---|---|
| `Result.Ok<T>(T value)` | `Result<T>` (success) |
| `Result.Fail<T>(string error)` | `Result<T>` (failure) |

### Usage example (handler)

```csharp
// Success — return the DTO
return Result<WorkflowDto>.Ok(dto);

// Failure — return an error message
return Result<WorkflowDto>.Fail("Workflow not found");
```

### Accessing Value safely

`Value` must only be accessed after confirming `IsSuccess` is `true`. `HandleResult` in
`BaseApiController` performs this check. Application code outside the API layer should
guard with `if (result.IsSuccess)` before reading `result.Value`.
