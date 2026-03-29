# Dev Auth Stub

> Last updated: 2026-03-28
> Phase: 1
> Status: Complete — PR approved

---

## What it does

Phase 1 has no real authentication system — that is Phase 2 work. Without any auth plumbing
in place, every subsequent feature would either need to skip authentication entirely or
carry its own provisional wiring. The Dev Auth Stub solves this by providing a single
hardcoded developer identity that can log in via a one-click page, receive a genuine
signed JWT, and use it on every subsequent API request — exactly as the real system will
work in Phase 2. The stub is gated behind environment checks on both backend and frontend
so it cannot exist in a production build. It is designed to be deleted in one operation
when Phase 2 real auth replaces it.

---

## How it works

When a developer navigates to `/dev-login` and clicks "Log in as dev user", the
`DevLoginPage` component calls `useDevLogin`, which fires a React Query mutation to
`authService.devLogin()`. That calls `POST /api/auth/dev-login` on the API. The
`AuthController` checks `IHostEnvironment.IsDevelopment()` — if true, it builds an HS256
JWT containing hardcoded stub claims (`sub`, `email`, `role`, `workspaceId`, `iat`, `exp`),
signs it with the `DevAuth:JwtSecret` from `appsettings.Development.json`, and returns the
token with a 24-hour expiry. On success, `useDevLogin` dispatches `setCredentials` to the
Redux auth slice, which stores the token and identity fields in memory. The `apiClient`
Axios instance has a request interceptor that reads `store.getState().auth.accessToken`
synchronously before every outgoing request and attaches it as `Authorization: Bearer
{token}`. Any controller decorated with `[Authorize]` will accept requests from this point
forward. The `GET /api/auth/me` endpoint exists to verify the token is valid and the claims
are correct — it reads the identity directly from `HttpContext.User` with no database access.

---

## Key files

| File | Purpose |
|---|---|
| `web-api/src/StackFlow.Api/Controllers/AuthController.cs` | Both endpoints: `POST /api/auth/dev-login` and `GET /api/auth/me`. JWT generation is a private method in this file. |
| `web-api/src/StackFlow.Api/Program.cs` | JWT bearer authentication registration. Custom `OnChallenge` handler that returns `{ "error": "Unauthorised" }` on 401. |
| `web-api/tests/StackFlow.IntegrationTests/Api/AuthEndpointTests.cs` | Integration tests covering all four acceptance criteria: 200 in Development, claims round-trip, 401 without token, 403 in Production. |
| `web-frontend/src/modules/auth/dtos/AuthDto.ts` | TypeScript interfaces `DevLoginResponseDto` and `MeDto` — the source of truth for the API contract on the frontend. |
| `web-frontend/src/modules/auth/infrastructure/auth-service.ts` | `authService` — the only file that knows the auth API endpoints exist. `devLogin()` and `me()` functions. |
| `web-frontend/src/modules/auth/hooks/useDevLogin.ts` | React Query `useMutation` hook. Calls `authService.devLogin()`, dispatches `setCredentials` to Redux, navigates to `/`, shows Sonner toast on error. |
| `web-frontend/src/modules/auth/ui/pages/DevLoginPage.tsx` | Single-button dev login page. Has a dual DEV guard: the route guard (`DevRoute`) prevents mounting; an internal guard redirects immediately if somehow reached in production. |
| `web-frontend/src/store/authSlice.ts` | Redux slice. State: `accessToken`, `userId`, `email`, `role`, `workspaceId` (all nullable). Actions: `setCredentials`, `clearCredentials`. Selector: `selectAuth`. |
| `web-frontend/src/store/store.ts` | Redux store with `auth` reducer registered. Exports `RootState` and `AppDispatch`. |
| `web-frontend/src/store/hooks.ts` | Typed `useAppDispatch` and `useAppSelector` hooks. All Redux consumers use these instead of the untyped react-redux originals. |
| `web-frontend/src/router/index.tsx` | Route registry. `/dev-login` is wrapped in `DevRoute`. `/` is a placeholder pending Feature 6 (App Shell). |
| `web-frontend/src/router/guards/DevRoute.tsx` | Route guard component. Redirects to `/` when `import.meta.env.DEV` is false. |
| `web-frontend/src/modules/shared/infrastructure/api-client.ts` | Axios instance. Request interceptor added in this feature: reads `store.getState().auth.accessToken` and attaches `Authorization: Bearer {token}` on every outgoing request. |

---

## Database changes

No database changes. No tables created, no migrations added. The stub identity is entirely
in-memory and in-code — there is no User entity at this stage.

---

## Events

No RabbitMQ events in this feature.

---

## Real-time (SignalR)

No SignalR events in this feature.

---

## Key design decisions

**JWT generation lives in the controller, not a handler.** There are no domain entities,
no repository access, and no business rules. A handler would add infrastructure for zero
gain. The controller comment documents this explicitly so the decision is not re-litigated.

**Token stored in Redux memory only, not localStorage.** The token is lost on page refresh.
This is intentional for a dev stub — it keeps the implementation minimal and avoids
localStorage coupling. Phase 2 will add persistence.

**Dual DEV guard (route + component).** `DevRoute` prevents the page component from
mounting at all in production. `DevLoginPage` also has an internal `Navigate` redirect.
Both checks are in place because the route guard alone could theoretically be bypassed if
the component were rendered outside the router.

**`IHostEnvironment.IsDevelopment()` check on every call, not at startup.** The endpoint
is registered unconditionally in the route table but the controller blocks it at request
time. This means the route is harmless even if reachable in a non-Development environment —
it will always return 403.

**`apiClient` reads the Redux store directly, not via a hook.** Request interceptors run
outside the React component tree and cannot use hooks. `store.getState()` is the correct
synchronous accessor for this context.

---

## Known limitations or caveats

- The token is lost on page refresh because it is stored in Redux memory only. A developer
  who refreshes the browser must click "Log in as dev user" again. This is expected behaviour
  for the stub.
- No logout endpoint. Clearing Redux state via `clearCredentials` is sufficient — call
  `dispatch(clearCredentials())` from any component to clear the session.
- `ClockSkew` is set to `TimeSpan.Zero` in the JWT bearer middleware. A token issued at
  exactly the expiry boundary will be rejected with no grace period.
- The `ValidateIssuer` and `ValidateAudience` parameters are both `false`. Phase 2 will
  enable these once the real token issuer is established.

---

## Notes: Brief vs implementation

The implementation matches the Feature Brief with one minor addition not mentioned in the
brief: `DevLoginPage` uses a two-component structure (`DevLoginPage` as the guard wrapper,
`DevLoginContent` as the inner component that calls `useDevLogin`). This was required to
satisfy the Rules of Hooks — a hook cannot be called before a conditional return, so the
hook call was moved to a child component that only renders after the DEV check has passed.
The behaviour is identical to what the brief described.
