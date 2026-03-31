# Landing Page API Reference

> Last updated: 2026-03-31
> Feature status: Complete — PR approved
> Note: This feature is frontend-only. No new endpoints were introduced. This reference documents the one existing endpoint that the landing page consumes.

---

## POST /api/auth/dev-login

Authenticates a user for development mode using a hardcoded dev identity. This endpoint
is only available when the API is running in the Development environment. It requires
no credentials — the server generates a JWT with a pre-set dev user identity.

**Auth:** Public (no token required)

### Request body

No request body. The request payload is empty.

### Response 200

| Field | Type | Description |
|---|---|---|
| `accessToken` | string | JWT bearer token. Used in all subsequent authenticated requests via the `Authorization: Bearer {token}` header. |
| `expiresAt` | string (ISO 8601) | Expiration timestamp of the token. Example: `2026-03-31T18:30:00Z` |
| `user` | object | User identity object returned by the token. |
| `user.id` | string (UUID) | Unique identifier of the user. Example: `550e8400-e29b-41d4-a716-446655440000` |
| `user.email` | string | Email address of the dev user. Example: `dev@stackflow.local` |
| `user.role` | string | Authorization role. Example: `Admin` or `User` |
| `user.workspaceId` | string (UUID) | Workspace the user belongs to. Example: `550e8400-e29b-41d4-a716-446655440001` |

### Error responses

| Status | Body | When |
|---|---|---|
| 500 | `{ "error": string }` | API encountered an unexpected error (e.g. database unavailable, token generation failed) |

### Example

**Request:**
```bash
POST http://localhost:5000/api/auth/dev-login
Content-Type: application/json
```

Body: (empty)

**Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJlbWFpbCI6ImRldkBzdGFja2Zsb3cubG9jYWwiLCJyb2xlIjoiQWRtaW4iLCJ3b3Jrc3BhY2VJZCI6IjU1MGU4NDAwLWUyOWItNDFkNC1hNzE2LTQ0NjY1NTQ0MDAwMSIsImV4cCI6MTc0MzUyMDYwMH0.signature",
  "expiresAt": "2026-03-31T18:30:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "dev@stackflow.local",
    "role": "Admin",
    "workspaceId": "550e8400-e29b-41d4-a716-446655440001"
  }
}
```

---

## Usage in the frontend

The landing page consumes this endpoint via the `useDevLogin` hook:

```typescript
const login = useDevLogin();

const handleEnterApp = () => {
  login.mutate(undefined);  // POST /api/auth/dev-login
};
```

On success, the hook dispatches the response to Redux (`setCredentials`) and navigates
to `/workflows`. On error, it displays a Sonner toast: "Could not connect to the server.
Is the API running?"

---

## Phase 1 scope note

This endpoint was introduced in Feature 2 (Dev Auth Stub) and is not new in Feature 7.
Feature 7 (Landing Page) is the first public-facing UI that uses this endpoint. In Phase 1,
dev login is the only authentication method. Real authentication (Email+Password, Google OAuth, OTP)
is planned for Phase 2.
