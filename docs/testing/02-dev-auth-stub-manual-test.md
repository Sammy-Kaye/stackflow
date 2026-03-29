# Manual Test Checklist — Feature 2: Dev Auth Stub
Phase: 1
Feature: Dev Auth Stub
Date written: 2026-03-28

---

## Prerequisites

Before running any step, confirm the following are true:

- [ ] The API is running locally. Start it with `dotnet run` from
      `web-api/src/StackFlow.Api/`, or press F5 in your IDE.
      The API must be listening on `http://localhost:5000`.
- [ ] `ASPNETCORE_ENVIRONMENT` is set to `Development` (the `http` launch profile in
      `launchSettings.json` sets this automatically).
- [ ] The frontend dev server is running. Start it with `npm run dev` from
      `web-frontend/`. It must be accessible at `http://localhost:3000`.
- [ ] A tool to inspect JWT tokens is available. Use `https://jwt.io` (paste a token
      into the "Encoded" field and the claims appear on the right).

---

## Section A — Backend: Swagger UI is accessible

**Step 1 — Swagger loads in the browser**

1. Open `http://localhost:5000/swagger` in a browser.
2. Pass criteria: The Swagger UI page loads. The title reads "StackFlow API v1".
   Two endpoints are visible under the `Auth` group:
   - `POST /api/auth/dev-login`
   - `GET /api/auth/me`
3. Fail criteria: 404 page, blank page, or connection refused.

---

## Section B — Backend: POST /api/auth/dev-login — happy path

**Step 2 — Dev login returns HTTP 200 with a token**

1. In Swagger UI, click `POST /api/auth/dev-login`, then click "Try it out".
2. Clear the request body field entirely (leave it empty — this endpoint takes no body).
3. Click "Execute".
4. Pass criteria: Response code is `200`. Response body matches this shape exactly:

```json
{
  "accessToken": "<a non-empty string>",
  "expiresAt": "<ISO 8601 timestamp>",
  "user": {
    "id": "00000000-0000-0000-0000-000000000001",
    "email": "dev@stackflow.local",
    "role": "Admin",
    "workspaceId": "00000000-0000-0000-0000-000000000002"
  }
}
```

5. Fail criteria: Any status other than 200, missing fields, or incorrect id/email/role/workspaceId values.

**Step 3 — expiresAt is approximately 24 hours in the future**

1. Copy the `expiresAt` value from the Step 2 response (example: `2026-03-29T10:00:00.0000000Z`).
2. Compare it to the current UTC time.
3. Pass criteria: The timestamp is between 23 hours 55 minutes and 24 hours 5 minutes
   from now. Minor clock skew of a few seconds is acceptable.
4. Fail criteria: Timestamp is in the past, or more than a few minutes off from 24 hours.

**Step 4 — Token contains the correct claims**

1. Copy the `accessToken` value from the Step 2 response (the full JWT string).
2. Open `https://jwt.io` and paste the token into the "Encoded" field.
3. In the "Payload" panel on the right, confirm the following claims are present:

| Claim | Expected value |
|---|---|
| `sub` | `00000000-0000-0000-0000-000000000001` |
| `email` | `dev@stackflow.local` |
| `role` (or `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`) | `Admin` |
| `workspaceId` | `00000000-0000-0000-0000-000000000002` |
| `iat` | A Unix timestamp close to the current time |
| `exp` | A Unix timestamp approximately 24 hours after `iat` |

4. Pass criteria: All six claims are present with the values listed above.
5. Fail criteria: Any claim is missing, or any value does not match.

---

## Section C — Backend: GET /api/auth/me — happy path

**Step 5 — /api/auth/me returns the correct identity when given a valid token**

1. Copy the `accessToken` value from Step 2.
2. In Swagger UI, click the "Authorize" button (padlock icon, top right of the page).
3. In the "Value" field, type `Bearer ` followed by the token (example:
   `Bearer eyJhbGci...`). Click "Authorize", then "Close".
4. Click `GET /api/auth/me`, then "Try it out", then "Execute".
5. Pass criteria: Response code is `200`. Response body is:

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "email": "dev@stackflow.local",
  "role": "Admin",
  "workspaceId": "00000000-0000-0000-0000-000000000002"
}
```

6. Fail criteria: Any status other than 200, or values that differ from Step 2.

---

## Section D — Backend: GET /api/auth/me — no token (failure case)

**Step 6 — /api/auth/me returns 401 when no token is provided**

1. In Swagger UI, click the "Authorize" button. If a token is present from Step 5,
   click "Logout" to clear it. Click "Close".
2. Click `GET /api/auth/me`, then "Try it out", then "Execute".
3. Pass criteria: Response code is `401`. Response body is:

```json
{
  "error": "Unauthorised"
}
```

4. Fail criteria: Any other status code, or a different response body shape.

Alternatively, run the same check with curl:
```
curl -i http://localhost:5000/api/auth/me
```
Expected: `HTTP/1.1 401` with `{"error":"Unauthorised"}` in the body.

---

## Section E — Backend: POST /api/auth/dev-login — Production environment (failure case)

**Step 7 — Dev login returns 403 when not in Development environment**

This test requires temporarily changing the environment. The simplest way:

1. Stop the API if it is currently running.
2. In a terminal, run the API with the environment overridden:
   ```
   cd web-api/src/StackFlow.Api
   ASPNETCORE_ENVIRONMENT=Production dotnet run
   ```
   On Windows (PowerShell):
   ```
   $env:ASPNETCORE_ENVIRONMENT="Production"
   dotnet run
   ```
3. Note: Swagger UI will not load in Production — that is expected behaviour.
   Use curl or any HTTP client instead.
4. Send the request:
   ```
   curl -i -X POST http://localhost:5000/api/auth/dev-login
   ```
5. Pass criteria: Response code is `403`. Response body is:

```json
{
  "error": "Dev login is not available in this environment"
}
```

6. Fail criteria: Any other status code, or a 200 response with a token.
7. After the test, stop the API and restart it normally (Development environment) before continuing.

---

## Section F — Frontend: /dev-login page loads and performs login

**Step 8 — The Dev Login page is visible in the browser**

1. Ensure the frontend dev server is running at `http://localhost:3000`.
   Ensure the API is running in Development at `http://localhost:5000`.
2. Open `http://localhost:3000/dev-login` in a browser.
3. Pass criteria: A page loads showing the StackFlow heading, the subtitle
   "Development environment — stub login only", and a button labelled
   "Log in as dev user".
4. Fail criteria: Blank page, 404, or redirect to `/`.

**Step 9 — Clicking the button logs in and redirects to /**

1. On the `/dev-login` page from Step 8, click "Log in as dev user".
2. Pass criteria:
   - The button label briefly changes to "Logging in..." while the request is in flight.
   - The browser navigates to `http://localhost:3000/`.
   - No error toast appears.
3. Fail criteria: Button stays disabled, error toast appears, or page does not redirect.

**Step 10 — Redux auth store is populated after login**

1. Open the browser developer tools (F12). Go to the Redux DevTools panel.
   (If Redux DevTools extension is not installed, install it from the Chrome or
   Firefox extension store.)
2. After completing Step 9, inspect the Redux state tree under `auth`.
3. Pass criteria: The `auth` slice contains:

| Field | Expected value |
|---|---|
| `accessToken` | A non-empty JWT string |
| `userId` | `00000000-0000-0000-0000-000000000001` |
| `email` | `dev@stackflow.local` |
| `role` | `Admin` |
| `workspaceId` | `00000000-0000-0000-0000-000000000002` |

4. Fail criteria: Any field is null or contains an incorrect value.

---

## Section G — Frontend: apiClient attaches the Bearer token

**Step 11 — Subsequent API calls include the Authorization header**

1. After completing Step 9 (you are logged in and on `/`), open the browser
   developer tools Network tab.
2. Trigger any API call. The simplest way is to navigate back to `/dev-login`
   temporarily and then forward again — or open the browser console and type:
   ```javascript
   fetch('http://localhost:5000/api/auth/me', {
     headers: { Authorization: `Bearer ${window.__REDUX_DEVTOOLS_EXTENSION__
       ? JSON.parse(localStorage.getItem('persist:root') || '{}').auth
       : '(check Redux DevTools for the token)'}` }
   })
   ```
   A simpler approach: open the Network tab, then hard-refresh the page. Any
   request that the app fires automatically will appear.

   The most reliable approach: in the browser console, call the dev login again
   and watch the Network tab for the POST request to `/api/auth/dev-login`. Then
   call `/api/auth/me` from the console:

   Open browser console and run:
   ```javascript
   // Read the token from Redux state via the app's store (if exposed on window)
   // If not exposed, check the Network tab after any authenticated call fires.
   ```

   Alternatively: after logging in via the button, observe the Network tab for
   the POST `/api/auth/dev-login` call. Then navigate to any page that triggers
   a protected API call — check the Request Headers for `Authorization: Bearer ...`.

3. Pass criteria: The `Authorization: Bearer {token}` header is present on the
   request to `/api/auth/me` (or any other API call made after login).
4. Fail criteria: The `Authorization` header is absent or the value is not a
   valid bearer token.

Note: A direct way to verify this without navigating further is to use curl with
the token retrieved from Redux DevTools:
```
curl -i http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer {paste token here}"
```
Pass criteria: HTTP 200 with the correct identity body (same as Step 5).

---

## Section H — Frontend: /dev-login is not accessible in a production build

**Step 12 — Production build redirects /dev-login to /**

This test requires building the frontend for production. This is optional if
you want a fast smoke-test — skip it if you are testing on the dev server only.

1. From `web-frontend/`, run:
   ```
   npm run build
   npm run preview
   ```
2. Open the URL shown by `npm run preview` (typically `http://localhost:4173`).
3. Navigate to `http://localhost:4173/dev-login`.
4. Pass criteria: The browser immediately redirects to `/` and the Dev Login page
   is not rendered. No "Log in as dev user" button is visible.
5. Fail criteria: The Dev Login page renders in a production build.

---

## Summary of acceptance criteria coverage

| AC # | Description | Test steps |
|---|---|---|
| AC 1 | POST /api/auth/dev-login returns 200 with correct body | Steps 2, 3 |
| AC 2 | GET /api/auth/me returns 200 with correct claims when given valid token | Steps 4, 5 |
| AC 3 | GET /api/auth/me returns 401 with no token | Step 6 |
| AC 4 | POST /api/auth/dev-login returns 403 in non-Development environment | Step 7 |
| AC 5 | Frontend /dev-login button populates Redux store and redirects to / | Steps 8, 9, 10 |
| AC 6 | apiClient attaches Authorization header on all subsequent requests | Step 11 |
| AC 7 | /dev-login redirects to / in a production build | Step 12 |
| AC 8 | Token contains correct claims (sub, email, role, workspaceId, iat, exp) | Step 4 |
