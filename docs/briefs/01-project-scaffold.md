# Feature Brief: Project Scaffold
Phase: 1
Status: Ready for implementation

---

## What this feature does (plain English)

This feature creates the runnable skeleton of the entire StackFlow system. Before any
business logic can be written, the physical project structure must exist: four .NET 9
class library and web projects wired together in a solution file, a React 19 + TypeScript
frontend created with Vite, and a Docker Compose file that brings up PostgreSQL, RabbitMQ,
the API, and the frontend with a single command. When this feature is done, a developer
can clone the repo, run `docker compose up -d`, hit `http://localhost:5000/health`, and
receive a 200 OK — proving the entire stack is wired and talking.

---

## Scope — what IS in this brief

- Solution file `StackFlow.sln` at `web-api/`
- Four .NET 9 projects created with correct project references:
  - `StackFlow.Domain` (class library — no dependencies)
  - `StackFlow.Application` (class library — references Domain only)
  - `StackFlow.Infrastructure` (class library — references Application and Domain)
  - `StackFlow.Api` (ASP.NET Core Web API — references Infrastructure and Application)
- Two test projects created with correct project references:
  - `StackFlow.UnitTests` (xUnit — references Application)
  - `StackFlow.IntegrationTests` (xUnit — references Api)
- NuGet packages added to each project as listed in the Package Manifest section below
- `Program.cs` in `StackFlow.Api` configured for Swagger, CORS, health checks, and JSON serialisation
- `appsettings.json` and `appsettings.Development.json` in `StackFlow.Api` with placeholder
  sections for ConnectionStrings, Jwt, RabbitMq, and Email — all values read from environment
  variables, no hardcoded secrets
- A single `GET /health` endpoint returning `{ "status": "healthy" }` with HTTP 200
- React 19 + TypeScript frontend scaffolded at `web-frontend/` using Vite (`react-ts` template)
- Frontend dependencies installed: React Query, Redux Toolkit, React Router, Axios, shadcn/ui
  (New York style), React Hook Form, Zod, date-fns, Sonner, React Flow, clsx, tailwind-merge
- Frontend directory structure created as per CLAUDE.md: `src/modules/shared/`, `src/store/`,
  `src/router/`, `src/design-reference/`
- `apiClient.ts` created in `src/modules/shared/infrastructure/` as the single Axios instance —
  base URL read from `VITE_API_URL` env var
- `docker-compose.yml` at the project root defining four services: `postgres`, `rabbitmq`, `api`, `frontend`
- `.env.example` at the project root documenting every required environment variable with a
  comment explaining what each one is
- `.gitignore` entries confirmed correct for .NET, Node, and Docker artefacts
- `Dockerfile` for the API at `web-api/Dockerfile`
- `Dockerfile` for the frontend at `web-frontend/Dockerfile`

---

## Scope — what is NOT in this brief

- No domain entities, EF Core DbContext, or migrations — that is Feature 3
- No repository interfaces or implementations — that is Feature 4
- No custom mediator or pipeline behaviors — that is Feature 5
- No authentication of any kind — a dev auth stub comes in Feature 2
- No business logic endpoints beyond `/health`
- No frontend pages, components, or routes beyond the bare Vite scaffold
- No Redux slices beyond the store wiring (slices are created per feature)
- No React Query configuration beyond installing the package (QueryClient is wired in Feature 6)
- No Terraform or Ansible — that is Phase 3
- No MinIO bucket setup or S3 configuration — that is Phase 2

---

## Domain entities involved

None. This feature creates the structural skeleton only. No domain entities are defined
or persisted in this brief.

---

## API Contract

### GET /health
Auth: Public

Response 200:
```
{
  status: string
}
```

Example response body:
```json
{ "status": "healthy" }
```

No request body. No error responses — this endpoint must always return 200 while the
process is running. If the process is down, the connection fails at the network level
before any response is returned.

---

## Frontend routes and views

No routes or views are created in this brief. The Vite scaffold produces a default
`App.tsx` and `main.tsx`. Both are left in place but cleaned of Vite boilerplate
(remove the counter demo, the default CSS, the Vite/React logos). Leave a single
`<h1>StackFlow</h1>` as the rendered output so the page is not blank.

The `src/router/` directory is created but the router is not wired until Feature 6
(App Shell + Routing). The `src/store/` directory is created but no slices are
defined until they are needed per feature.

---

## RabbitMQ events (if any)

None for this feature.

---

## SignalR events (if any)

None for this feature.

---

## Audit requirements

None. No state mutations occur in this feature. The `/health` endpoint is read-only
and does not involve any workflow or task entities.

---

## Acceptance criteria

1. Given the repository is cloned fresh, when `docker compose up -d` is run from the
   project root, then all four services (postgres, rabbitmq, api, frontend) start
   without errors and reach a healthy state within 60 seconds.

2. Given the stack is running, when `GET http://localhost:5000/health` is called, then
   the response is HTTP 200 with body `{ "status": "healthy" }`.

3. Given the stack is running, when `http://localhost:5000/swagger` is opened in a
   browser, then the Swagger UI loads and shows the health endpoint.

4. Given the stack is running, when `http://localhost:3000` is opened in a browser,
   then the React frontend loads and displays "StackFlow" with no console errors.

5. Given the stack is running, when `http://localhost:15672` is opened in a browser,
   then the RabbitMQ management UI loads and accepts login with guest / guest.

6. Given the solution is opened in a .NET IDE, when the solution is built with
   `dotnet build`, then it compiles with zero errors and zero warnings.

7. Given the frontend directory is opened in a terminal, when `npm run build` is run,
   then it compiles with zero TypeScript errors.

8. Given `.env.example` is read by a developer who has never seen the project, then
   every environment variable required to run the application is documented with a comment
   explaining what it is and what format it expects.

9. Given the project reference graph is inspected, then `StackFlow.Domain` has zero
   project references, `StackFlow.Application` references only Domain, `StackFlow.Infrastructure`
   references only Application (which transitively includes Domain), and `StackFlow.Api`
   references only Infrastructure (which transitively includes Application and Domain).

10. Given the repository is inspected, then no connection strings, passwords, API keys,
    or secrets appear anywhere in committed files — all sensitive values are in `.env.example`
    as placeholders only.

---

## Package Manifest

### StackFlow.Domain
No NuGet packages. Pure C#.

### StackFlow.Application
- FluentValidation (v11+)

### StackFlow.Infrastructure
- Microsoft.EntityFrameworkCore (v9)
- Npgsql.EntityFrameworkCore.PostgreSQL (v9)
- Microsoft.EntityFrameworkCore.Design (v9)
- RabbitMQ.Client (v6+)
- MailKit (v4+)
- AWSSDK.S3 (v3+)

### StackFlow.Api
- Microsoft.AspNetCore.OpenApi (v9)
- Swashbuckle.AspNetCore (v6+)
- Microsoft.AspNetCore.SignalR (included in ASP.NET Core — no extra package needed)
- Microsoft.AspNetCore.Authentication.JwtBearer (v9)

### StackFlow.UnitTests
- xUnit
- Moq (v4+)
- FluentAssertions (v6+)

### StackFlow.IntegrationTests
- xUnit
- Microsoft.AspNetCore.Mvc.Testing (v9)
- FluentAssertions (v6+)

### Frontend (npm)
- react, react-dom, typescript (via Vite template)
- @tanstack/react-query
- @reduxjs/toolkit, react-redux
- react-router-dom
- axios
- react-hook-form
- zod, @hookform/resolvers
- date-fns
- sonner
- reactflow
- clsx, tailwind-merge
- tailwindcss, @tailwindcss/vite (or postcss equivalent)
- shadcn/ui (via CLI init — New York style, CSS variables enabled)

---

## Docker Compose Service Definitions

### postgres
- Image: postgres:16-alpine
- Environment: POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD (from .env)
- Port: 5432:5432
- Volume: postgres_data

### rabbitmq
- Image: rabbitmq:3-management-alpine
- Port: 5672:5672 (AMQP), 15672:15672 (management UI)
- Default credentials: guest / guest (dev only)


### api
- Build: ./web-api
- Port: 5000:8080
- Depends on: postgres, rabbitmq
- Environment: all values from .env
- Health check: GET /health

### frontend
- Build: ./web-frontend
- Port: 3000:80
- Depends on: api

---

## Environment Variables (for .env.example)

```
# PostgreSQL
POSTGRES_DB=stackflow
POSTGRES_USER=stackflow
POSTGRES_PASSWORD=stackflow_dev

# API — Database
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=stackflow;Username=stackflow;Password=stackflow_dev

# API — JWT (placeholder values for dev; replace with real secrets in production)
Jwt__Secret=REPLACE_WITH_A_LONG_RANDOM_SECRET_MIN_32_CHARS
Jwt__Issuer=stackflow-api
Jwt__Audience=stackflow-app
Jwt__AccessTokenExpiryMinutes=60
Jwt__RefreshTokenExpiryDays=30

# API — RabbitMQ
RabbitMq__Host=rabbitmq
RabbitMq__Port=5672
RabbitMq__Username=guest
RabbitMq__Password=guest

# API — Email (Brevo SMTP — swap credentials for prod)
Email__SmtpHost=smtp-relay.brevo.com
Email__SmtpPort=587
Email__Username=REPLACE_WITH_BREVO_SMTP_USERNAME
Email__Password=REPLACE_WITH_BREVO_SMTP_KEY
Email__FromAddress=noreply@yourdomain.com
Email__FromName=StackFlow


# Frontend
VITE_API_URL=http://localhost:5000
```

---

## Agent instructions

**Backend Agent:** ordered build sequence

1. Create the solution file and four projects under `web-api/src/` and two test projects
   under `web-api/tests/` with the correct project reference graph as specified in
   Acceptance Criterion 9.
2. Add NuGet packages to each project as listed in the Package Manifest section.
3. Write `Program.cs` for `StackFlow.Api`: register Swagger, health checks, CORS (allow
   all origins for dev), and JSON serialisation options (camelCase, ignore null). Wire
   up the service container. Do not wire anything that does not exist yet (no DbContext,
   no mediator, no auth).
4. Create `appsettings.json` and `appsettings.Development.json` with placeholder sections
   for ConnectionStrings, Jwt, RabbitMq, Email, and Storage. All values read from
   environment variables — no hardcoded values.
5. Create the `GET /health` endpoint. This may be a minimal API endpoint or a thin
   controller returning `{ "status": "healthy" }`. Follow the thin controller pattern
   from CLAUDE.md if using a controller.
6. Write the `Dockerfile` for the API at `web-api/Dockerfile`. Use a multi-stage build:
   SDK image to build and publish, runtime image (`aspnet:9`) to run. Expose port 8080.

**Frontend Agent:** ordered build sequence

1. Wait for the Backend Agent to confirm the API Dockerfile and `GET /health` are in
   place before beginning. Frontend scaffolding does not depend on a running API, but
   the handoff confirmation is the signal to start.
2. Scaffold the Vite React TypeScript project at `web-frontend/` using
   `npm create vite@latest web-frontend -- --template react-ts`.
3. Install all npm packages listed in the Package Manifest section.
4. Initialise shadcn/ui via `npx shadcn@latest init` — New York style, CSS variables
   enabled, Tailwind configured.
5. Create the directory structure under `web-frontend/src/`: `modules/shared/infrastructure/`,
   `store/`, `router/`, `design-reference/`.
6. Create `src/modules/shared/infrastructure/api-client.ts` as the single Axios instance.
   Base URL is read from `import.meta.env.VITE_API_URL`. No other configuration at this
   stage.
7. Clean `App.tsx` of Vite boilerplate. Replace content with a single `<h1>StackFlow</h1>`.
   Remove default Vite CSS files. Do not wire the router or Redux store yet.
8. Write the `Dockerfile` for the frontend at `web-frontend/Dockerfile`. Use a multi-stage
   build: Node image to install and build, Nginx alpine image to serve the `dist/` output.
   Expose port 80.

**Shared (either agent — assign to Backend Agent for sequencing):**

9. Write `docker-compose.yml` at the project root using the service definitions in the
   Docker Compose Service Definitions section above.
10. Write `.env.example` at the project root using the variables in the Environment
    Variables section above. Every variable must have a comment explaining its purpose.
11. Confirm `.gitignore` at the project root covers: `*.user`, `bin/`, `obj/`,
    `node_modules/`, `.env`, `*.env.local`, Docker volume data directories.

**Handoff point:** Backend Agent must confirm the following before Frontend Agent begins:
- Solution builds with `dotnet build` returning zero errors
- `web-api/Dockerfile` is in place
- `docker-compose.yml` is in place
- `GET /health` returns 200

Samuel will relay this confirmation. Frontend Agent does not poll or assume — it waits
for Samuel's explicit "backend handoff complete" message.
