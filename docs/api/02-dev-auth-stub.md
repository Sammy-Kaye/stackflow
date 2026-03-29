# Dev Auth Stub — API Reference

> Last updated: 2026-03-28
> Feature status: Approved — PR reviewed
> Related files: web-api/src/StackFlow.Api/Controllers/AuthController.cs, web-api/src/StackFlow.Api/Program.cs

---

## POST /api/auth/dev-login

Issues a signed HS256 JWT for the hardcoded developer identity. This endpoint only exists
in the Development environment — it returns 403 in all other environments.

**Auth:** Public (no token required)

**Environment constraint:** Only active when `ASPNETCORE_ENVIRONMENT = Development`. The route
is registered unconditionally, but the controller checks `IHostEnvironment.IsDevelopment()`
on every call and returns 403 if the check fails. This is belt-and-suspenders: the endpoint
is harmless even if reachable outside Development.

### Request body

No request body.

### Response 200

| Field | Type | Description |
|---|---|---|
| `accessToken` | string | Signed HS256 JWT. Valid for 24 hours. |
| `expiresAt` | string | ISO 8601 timestamp (round-trip format with offset) indicating when the token expires. |
| `user.id` | string | Hardcoded stub user ID. Always `00000000-0000-0000-0000-000000000001`. |
| `user.email` | string | Hardcoded stub email. Always `dev@stackflow.local`. |
| `user.role` | string | Hardcoded stub role. Always `Admin`. |
| `user.workspaceId` | string | Hardcoded stub workspace ID. Always `00000000-0000-0000-0000-000000000002`. |

### Error responses

| Status | Body | When |
|---|---|---|
| 403 | `{ "error": "Dev login is not available in this environment" }` | `ASPNETCORE_ENVIRONMENT` is anything other than `Development` |

### JWT claims

The issued token contains the following claims:

| Claim | Value |
|---|---|
| `sub` | `00000000-0000-0000-0000-000000000001` |
| `email` | `dev@stackflow.local` |
| `role` | `Admin` |
| `workspaceId` | `00000000-0000-0000-0000-000000000002` |
| `iat` | Unix timestamp of issue time |
| `exp` | Unix timestamp 24 hours after `iat` |

Note: ASP.NET Core's JWT bearer middleware maps `sub` to `ClaimTypes.NameIdentifier` and
`email` to `ClaimTypes.Email` during validation. The `GET /api/auth/me` endpoint reads
claims using these mapped names.

### Example

**Request:**
```
POST /api/auth/dev-login
(no body)
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-03-29T10:00:00.0000000+00:00",
  "user": {
    "id": "00000000-0000-0000-0000-000000000001",
    "email": "dev@stackflow.local",
    "role": "Admin",
    "workspaceId": "00000000-0000-0000-0000-000000000002"
  }
}
```

---

## GET /api/auth/me

Returns the identity claims decoded from the bearer token. No database access — reads
directly from `HttpContext.User` (the claims principal populated by the JWT bearer middleware).

**Auth:** JWT required

### Request body

No request body.

### Response 200

| Field | Type | Description |
|---|---|---|
| `id` | string | The `sub` claim from the token (user ID as UUID string). |
| `email` | string | The `email` claim from the token. |
| `role` | string | The `role` claim from the token. |
| `workspaceId` | string | The `workspaceId` claim from the token. |

### Error responses

| Status | Body | When |
|---|---|---|
| 401 | `{ "error": "Unauthorised" }` | No `Authorization` header, expired token, or invalid signature |

Note: The 401 body is written by the `JwtBearerEvents.OnChallenge` handler registered in
`Program.cs`. ASP.NET Core's default 401 response has an empty body — the custom handler
overrides this to return consistent JSON.

### Example

**Request:**
```
GET /api/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "email": "dev@stackflow.local",
  "role": "Admin",
  "workspaceId": "00000000-0000-0000-0000-000000000002"
}
```

---

## Configuration

The JWT signing secret is read from `DevAuth:JwtSecret` in `appsettings.Development.json`.
The same key is used by both `AuthController` (signing) and the JWT bearer middleware in
`Program.cs` (validation).

In non-Development environments, `DevAuth:JwtSecret` is absent. `Program.cs` falls back to
a placeholder key in that case. No tokens are ever signed with the placeholder key because
the `POST /api/auth/dev-login` endpoint returns 403 before reaching token generation.

Phase 2 will replace the `DevAuth:JwtSecret` configuration with a `Jwt:Secret` key covering
the real authentication system.
