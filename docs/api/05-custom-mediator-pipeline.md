# Custom Mediator + Pipeline API Reference

> Last updated: 2026-03-30
> Feature status: ✅ Approved — PR reviewed
> Related files: `web-api/src/StackFlow.Application/Common/Mediator/`, `web-api/src/StackFlow.Api/Controllers/BaseApiController.cs`, `web-api/src/StackFlow.Api/Controllers/PingController.cs`

---

## Overview

This feature provides the CQRS request/response dispatch backbone for StackFlow. The custom hand-rolled mediator processes every command and query through a pipeline of behaviors (validation, logging) before reaching the handler. This is infrastructure-level work with one public endpoint (`GET /api/ping`) that serves as a smoke test.

---

## GET /api/ping

Smoke test endpoint. Dispatches `PingCommand` through the full mediator pipeline to verify end-to-end wiring.

**Auth:** JWT required (dev token from `POST /api/auth/dev-login`)

### Request body

No request body.

### Response 200

| Field | Type | Description |
|---|---|---|
| message | string | Always `"pong"` |

### Response 401

| Field | Type | Description |
|---|---|---|
| error | string | `"Unauthorised"` — missing or invalid JWT |

### Error responses

| Status | Body | When |
|---|---|---|
| 401 | `{ "error": "Unauthorised" }` | Missing or invalid JWT token |

### Example

**Request:**
```
GET /api/ping
Authorization: Bearer {valid-jwt}
```

**Response (200 OK):**
```json
{
  "message": "pong"
}
```

---

## Mediator Extension Points

This feature defines the interfaces and behaviors that all future commands and queries will use. Developers extend the system by:

1. **Creating a new command or query** — implement `ICommand<Result<T>>` or `IQuery<Result<T>>`
2. **Creating a handler** — implement `IRequestHandler<MyCommand, Result<T>>`
3. **Adding a validator (optional)** — implement `IValidator<MyCommand>` from FluentValidation; register in DI
4. **Dispatching from a controller** — call `HandleResult(await Mediator.Send(new MyCommand(...)))`

The mediator and pipeline behaviors are registered automatically via assembly scanning in `AddApplication()` — no manual wiring needed.

### Pipeline Behavior Order

When a request is dispatched:

```
Mediator.Send(request)
  ↓
ValidationBehavior     (runs validators, short-circuits if invalid)
  ↓
LoggingBehavior        (times the request, logs at Information or Warning)
  ↓
IRequestHandler        (your business logic)
```

If validation fails, the handler is never called — the failed result is returned immediately.

### HandleResult Status Code Mapping

The `BaseApiController.HandleResult()` method maps `Result<T>` to HTTP responses:

| Condition | HTTP Status | Body |
|---|---|---|
| `result.IsSuccess` | 200 OK | The value from `result.Value` |
| `result.IsFailure` and error contains "not found" (case-insensitive) | 404 Not Found | `{ "error": "..." }` |
| `result.IsFailure` and error contains "forbidden" (case-insensitive) | 403 Forbidden | (none) |
| `result.IsFailure` (any other error) | 400 Bad Request | `{ "error": "..." }` |

---
