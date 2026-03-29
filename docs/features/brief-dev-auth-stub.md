# Feature Brief: Dev Auth Stub
Phase: 1
Status: Complete — PR approved

---

## What this feature does (plain English)

During Phase 1 the real authentication system (JWT + refresh tokens, Google OAuth, OTP)
does not exist yet. Without any auth at all, every feature that comes after this one
would need to either skip authentication entirely or duplicate provisional wiring.
The Dev Auth Stub solves this by providing a single hardcoded developer identity that
can call a login endpoint, receive a real signed JWT, and use it on every subsequent
API request — exactly as the real auth system will work in Phase 2. The stub lives
behind an environment flag so it cannot exist in production, and every piece of it is
designed to be deleted in one operation when Phase 2 real auth is wired in.

## Scope — what IS in this brief

- A `POST /api/auth/dev-login` endpoint that accepts no credentials and returns a signed
  JWT access token containing a hardcoded developer user identity.
- The hardcoded stub identity: id `00000000-0000-0000-0000-000000000001`, email
  `dev@stackflow.local`, role `Admin`, workspace id `00000000-0000-0000-0000-000000000002`.
  These values are fixed constants — not read from the database.
- JWT signing uses a secret read from `appsettings.Development.json` under the key
  `DevAuth:JwtSecret`. The token must be a valid HS256-signed JWT that ASP.NET Core's
  JWT bearer middleware will accept.
- Token expiry: 24 hours (generous for dev convenience; not production-grade).
- JWT claims included: `sub` (user id), `email`, `role`, `workspaceId`, `iat`, `exp`.
- The endpoint is only registered when `ASPNETCORE_ENVIRONMENT` is `Development`. It
  must not exist, compile into, or be reachable in any other environment.
- JWT bearer authentication registered in `Program.cs` so that subsequent features can
  decorate controllers with `[Authorize]` and have it work immediately.
- A `GET /api/auth/me` endpoint (JWT required) that returns the decoded identity from
  the token — used to verify the token is valid and the claims are correct.
- Frontend `authService` with a `devLogin()` method that calls the stub endpoint.
- Redux auth slice (`store/authSlice`) with `setCredentials` and `clearCredentials`
  actions, storing `accessToken`, `userId`, `email`, `role`, and `workspaceId`.
- A `useDevLogin` hook that calls the service and dispatches to the Redux store.
- A minimal `DevLoginPage` at route `/dev-login` with a single "Log in as dev user"
  button. This page is only rendered when `import.meta.env.DEV` is true.
- The `apiClient` Axios instance configured with a request interceptor that reads the
  access token from the Redux store and attaches it as `Authorization: Bearer {token}`.

## Scope — what is NOT in this brief

- No real user database table, no User entity, no EF migration — the stub identity is
  entirely in-memory and in-code.
- No refresh token logic — the 24-hour JWT is the only token issued.
- No logout endpoint — clearing Redux state is sufficient for dev purposes.
- No password field on the login page — this is a zero-friction dev tool, not a login form.
- No role-based route guards on the frontend — that is part of Phase 2 auth.
- No Google OAuth, no OTP, no email+password — all Phase 2.
- No token storage in localStorage or cookies — the token lives in Redux in-memory only.
  It is lost on page refresh, which is acceptable for a dev stub.
- No Swagger JWT auth button configuration — keeping scope minimal.

## Domain entities involved

No domain entities from CLAUDE.md are involved. The stub user identity is a hardcoded
constant, not persisted to any entity or table. No new fields are added to any entity.

Note to Backend Agent: do not create a User entity here. That belongs to Phase 2.

## API Contract

#### POST /api/auth/dev-login
Auth: Public
Note: This endpoint only exists when ASPNETCORE_ENVIRONMENT = Development.

Request body: none (empty body or omitted)

Response 200:
{
  accessToken: string,
  expiresAt: string (ISO 8601),
  user: {
    id: string (uuid),
    email: string,
    role: string,
    workspaceId: string (uuid)
  }
}

Response 403: { error: "Dev login is not available in this environment" }

---

#### GET /api/auth/me
Auth: JWT required

Response 200:
{
  id: string (uuid),
  email: string,
  role: string,
  workspaceId: string (uuid)
}

Response 401: { error: "Unauthorised" }

## Frontend routes and views

Route added: `/dev-login`
- Only rendered when `import.meta.env.DEV` is true. In production builds this route
  must not exist. The router guard is a simple conditional — if not DEV, redirect to `/`.
- Component: `DevLoginPage` — located at
  `web-frontend/src/modules/auth/ui/pages/DevLoginPage.tsx`
- Contains one button: "Log in as dev user". On click, calls `useDevLogin`. On success,
  navigates to `/` (the app shell root). On error, shows a Sonner toast.

New module structure created by this feature:
```
web-frontend/src/modules/auth/
  dtos/          — AuthDto.ts  (DevLoginResponseDto, MeDto)
  infrastructure/ — auth-service.ts  (devLogin, me)
  hooks/          — useDevLogin.ts
  ui/pages/       — DevLoginPage.tsx

web-frontend/src/store/
  authSlice.ts
  store.ts  (if not already created by scaffold)
```

Existing files affected:
- `web-frontend/src/store/store.ts` — auth reducer added (or created if absent)
- `web-frontend/src/router/` — `/dev-login` route added
- `web-frontend/src/modules/shared/infrastructure/api-client.ts` — request interceptor
  added to attach `Authorization: Bearer {token}` header from Redux auth state

## RabbitMQ events (if any)

None for this feature.

## SignalR events (if any)

None for this feature.

## Audit requirements

None for this feature. No WorkflowState or WorkflowTaskState is mutated. The stub
login action does not touch any audited entity.

## Acceptance criteria

1. Given the API is running in Development environment, when `POST /api/auth/dev-login`
   is called with an empty body, then the response is HTTP 200 with a non-empty
   `accessToken` string, an `expiresAt` timestamp approximately 24 hours in the future,
   and a `user` object containing id `00000000-0000-0000-0000-000000000001`, email
   `dev@stackflow.local`, role `Admin`, and workspaceId
   `00000000-0000-0000-0000-000000000002`.

2. Given a valid access token returned by `POST /api/auth/dev-login`, when
   `GET /api/auth/me` is called with the token in the `Authorization: Bearer` header,
   then the response is HTTP 200 with the same id, email, role, and workspaceId as
   returned by the login endpoint.

3. Given a request to `GET /api/auth/me` with no Authorization header, then the
   response is HTTP 401 with body `{ "error": "Unauthorised" }`.

4. Given the API is running in any environment other than Development, when
   `POST /api/auth/dev-login` is called, then the response is HTTP 403 with body
   `{ "error": "Dev login is not available in this environment" }`.

5. Given the frontend is running in dev mode (`import.meta.env.DEV` is true), when
   the user navigates to `/dev-login` and clicks "Log in as dev user", then the Redux
   auth store is populated with `accessToken`, `userId`, `email`, `role`, and
   `workspaceId`, and the user is redirected to `/`.

6. Given the Redux auth store contains an access token, when any subsequent API call
   is made via `apiClient`, then the request includes the header
   `Authorization: Bearer {token}`.

7. Given the frontend is running in a production build (`import.meta.env.DEV` is
   false), when the user navigates to `/dev-login`, then they are redirected to `/`
   and the Dev Login page is not rendered.

8. Given a valid access token returned by the stub, when it is decoded, then it
   contains claims: `sub` equal to the stub user id, `email`, `role`, `workspaceId`,
   `iat`, and `exp` set to 24 hours after issue.

## Agent instructions

Backend Agent:
1. Add `Microsoft.AspNetCore.Authentication.JwtBearer` NuGet package to
   `StackFlow.Api`.
2. Add `DevAuth:JwtSecret` to `appsettings.Development.json` (a sufficiently long
   random string — minimum 32 characters).
3. Register JWT bearer authentication in `Program.cs`, reading the secret from
   configuration, with validation parameters matching what the stub will sign.
4. Create `StackFlow.Api/Controllers/AuthController.cs` inheriting `BaseApiController`.
5. Implement `POST /api/auth/dev-login`: check `IHostEnvironment.IsDevelopment()`;
   if false, return 403; if true, build the JWT with the hardcoded claims and return
   the `DevLoginResponseDto`.
6. Implement `GET /api/auth/me`: decorated with `[Authorize]`; reads claims from
   `HttpContext.User` and returns them as `MeDto`.
7. Do not create any database tables, migrations, or domain entities.
8. Ensure the project builds and both endpoints respond correctly.

Frontend Agent:
1. Wait for Backend Agent to confirm the two endpoints are live before writing
   implementation code.
2. Create `web-frontend/src/modules/auth/dtos/AuthDto.ts` with
   `DevLoginResponseDto` and `MeDto` types matching the API contract exactly.
3. Create `web-frontend/src/modules/auth/infrastructure/auth-service.ts` with
   `devLogin()` and `me()` functions using `apiClient`.
4. Create or confirm `web-frontend/src/store/authSlice.ts` with `setCredentials`
   and `clearCredentials` actions, typed state containing `accessToken`, `userId`,
   `email`, `role`, `workspaceId` (all nullable — null when not logged in).
5. Wire the auth reducer into the Redux store.
6. Add the `Authorization: Bearer` request interceptor to `apiClient` — reads token
   from the Redux store state directly (not from localStorage).
7. Create `web-frontend/src/modules/auth/hooks/useDevLogin.ts`.
8. Create `web-frontend/src/modules/auth/ui/pages/DevLoginPage.tsx` with the single
   button and the DEV-only guard.
9. Register the `/dev-login` route in the router, with the conditional guard.

Handoff point: Backend Agent must confirm that `POST /api/auth/dev-login` returns HTTP
200 with a valid JWT and `GET /api/auth/me` returns HTTP 200 with correct claims when
given that token. Frontend Agent begins implementation only after this confirmation.
