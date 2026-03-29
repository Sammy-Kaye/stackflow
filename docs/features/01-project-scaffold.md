# Project Scaffold

> Last updated: 2026-03-28
> Phase: 1, Feature 1
> Status: Complete — PR approved

---

## What it does

The project scaffold creates the runnable skeleton of the entire StackFlow system. No
business logic exists yet — this feature exists to prove that all four layers of the
backend, both test projects, the React frontend, and all four Docker Compose services
can be assembled and started cleanly. When this feature is complete, a developer can
clone the repository, run `docker compose up -d`, and reach a working API at
`http://localhost:5000/health` and a working frontend at `http://localhost:3000`.

---

## How it works

The backend is a four-project .NET 10 solution following Clean Architecture. The projects
are wired with the correct reference graph (Domain has no references; Application references
Domain; Infrastructure references Application; Api references Infrastructure). `Program.cs`
registers only what exists at this stage: controllers, Swagger, health checks, CORS, and
JSON serialisation. The single `GET /health` endpoint returns `{ "status": "healthy" }`.
The frontend is a Vite React 19 TypeScript project with all required packages installed and
a single Axios instance (`apiClient`) ready for the service layer. Both the API and frontend
are containerised with multi-stage Dockerfiles. Docker Compose brings up all four services —
PostgreSQL, RabbitMQ, the API, and the frontend — with health checks and correct startup
ordering. Every required environment variable is documented in `.env.example` with a comment
explaining its purpose and format.

---

## Key files

| File | Purpose |
|---|---|
| `web-api/StackFlow.sln` | Solution file — references all four source projects and two test projects |
| `web-api/src/StackFlow.Domain/` | Domain layer — no dependencies, no packages; pure C# |
| `web-api/src/StackFlow.Application/` | Application layer — references Domain; FluentValidation installed |
| `web-api/src/StackFlow.Infrastructure/` | Infrastructure layer — references Application; EF Core, Npgsql, RabbitMQ.Client, MailKit, AWSSDK.S3 installed |
| `web-api/src/StackFlow.Api/Program.cs` | API entry point — registers controllers, Swagger, CORS, health checks, JSON options |
| `web-api/src/StackFlow.Api/Controllers/BaseApiController.cs` | Abstract base controller — provides `HandleResult()` for mapping `Result<T>` to HTTP responses |
| `web-api/src/StackFlow.Api/Controllers/HealthController.cs` | Single `GET /health` endpoint |
| `web-api/src/StackFlow.Application/Common/Result.cs` | `Result` and `Result<T>` — the error handling primitives used by every handler |
| `web-api/tests/StackFlow.UnitTests/` | xUnit test project — references Application; Moq and FluentAssertions installed |
| `web-api/tests/StackFlow.IntegrationTests/` | xUnit test project — references Api; Microsoft.AspNetCore.Mvc.Testing and FluentAssertions installed |
| `web-api/Dockerfile` | Multi-stage build: SDK image to compile and publish, `aspnet:9` runtime image to run; exposes port 8080 |
| `web-frontend/` | React 19 + TypeScript project scaffolded with Vite (`react-ts` template) |
| `web-frontend/src/modules/shared/infrastructure/api-client.ts` | Single Axios instance; base URL read from `VITE_API_URL` at build time |
| `web-frontend/src/modules/shared/` | Shared utilities directory — cross-feature code lives here |
| `web-frontend/src/store/` | Redux store directory — slices are added per feature |
| `web-frontend/src/router/` | Router directory — wired in Feature 6 (App Shell + Routing) |
| `web-frontend/src/design-reference/` | Read-only Stitch design exports |
| `web-frontend/Dockerfile` | Multi-stage build: Node image to install and build, Nginx alpine to serve `dist/`; exposes port 80 |
| `docker-compose.yml` | Defines all four services with health checks and startup dependencies |
| `.env.example` | Documents every required environment variable with comments |

---

## Database changes

No database changes in this feature. No DbContext, no migrations, no tables. EF Core is
installed in `StackFlow.Infrastructure` but not configured. That is Feature 3 (Domain
Entities + DB).

---

## Docker Compose services

| Service | Image | Host port | Internal port | Depends on |
|---|---|---|---|---|
| `postgres` | `postgres:16-alpine` | 5432 | 5432 | — |
| `rabbitmq` | `rabbitmq:3-management-alpine` | 5672, 15672 | 5672, 15672 | — |
| `api` | Built from `./web-api/Dockerfile` | 5000 | 8080 | `postgres` (healthy), `rabbitmq` (healthy) |
| `frontend` | Built from `./web-frontend/Dockerfile` | 3000 | 80 | `api` |

The `api` container health check polls `GET http://localhost:8080/health` every 15 seconds
with a 30-second start period to allow the process to initialise.

PostgreSQL data persists in a named Docker volume (`postgres_data`). Run
`docker compose down -v` to destroy volumes and start fresh.

---

## How to run locally

```bash
# From the project root
cp .env.example .env        # copy and fill in any REPLACE_WITH_* values
docker compose up -d        # start all four services

# Verify:
# API health:    http://localhost:5000/health       → { "status": "healthy" }
# Swagger UI:    http://localhost:5000/swagger
# Frontend:      http://localhost:3000              → page displays "StackFlow"
# RabbitMQ UI:   http://localhost:15672             → login: guest / guest
```

The `.env` file is excluded from git. Never commit it.

---

## Environment variables

All variables are documented in `.env.example`. The groups are:

| Group | Variables | Used from |
|---|---|---|
| PostgreSQL | `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | `postgres` service |
| Database connection | `ConnectionStrings__DefaultConnection` | `api` service (EF Core — Feature 3) |
| JWT | `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__AccessTokenExpiryMinutes`, `Jwt__RefreshTokenExpiryDays` | `api` service (auth — Phase 2) |
| RabbitMQ | `RabbitMq__Host`, `RabbitMq__Port`, `RabbitMq__Username`, `RabbitMq__Password` | `api` service (messaging — Phase 2) |
| Email | `Email__SmtpHost`, `Email__SmtpPort`, `Email__Username`, `Email__Password`, `Email__FromAddress`, `Email__FromName` | `api` service (email — Phase 2) |
| Object storage | `Storage__ServiceUrl`, `Storage__AccessKey`, `Storage__SecretKey`, `Storage__BucketName`, `Storage__Region` | `api` service (file attachments — Phase 2) |
| Frontend | `VITE_API_URL` | `frontend` service build; `api-client.ts` at runtime |

JWT, RabbitMQ, Email, and Storage variables are declared in `appsettings.json` placeholder
sections and passed through Docker Compose, but none of the corresponding services are
wired in code at this stage.

---

## Frontend packages installed

| Package | Purpose |
|---|---|
| `axios` | HTTP client — used exclusively via `api-client.ts` |
| `@tanstack/react-query` | Server state management — wired in Feature 6 |
| `@reduxjs/toolkit`, `react-redux` | Client state — wired in Feature 6; slices added per feature |
| `react-router-dom` | Routing — wired in Feature 6 |
| `react-hook-form`, `zod`, `@hookform/resolvers` | Forms and validation — used per feature |
| `date-fns` | Date formatting — used per feature |
| `sonner` | Toast notifications — used per feature |
| `reactflow` | Workflow canvas — used in Feature 9 (Workflow Builder UI) |
| `clsx`, `tailwind-merge` | CSS class utilities |
| `tailwindcss` | Utility CSS framework |
| shadcn/ui (New York style) | Component library — initialised via CLI, CSS variables enabled |

---

## Events

No RabbitMQ events in this feature.

---

## Real-time (SignalR)

No SignalR events in this feature.

---

## Known limitations or caveats

**React Flow version compatibility risk.** The installed package is `reactflow` v11. React
19 removed `ReactDOM.render`, which `reactflow` v11 depends on internally. This will cause
a runtime failure when the workflow canvas is rendered in Feature 9 (Workflow Builder UI).
Before Feature 9 is built, `reactflow` must be replaced with `@xyflow/react` (v12+), which
is the maintained successor package that supports React 19's `createRoot`. No code change
is needed now — this is a pre-Feature-9 dependency upgrade task.

**CORS policy.** The `DevCors` policy allows all origins, methods, and headers. This is
intentional for local development. It must be replaced with an explicit origin allowlist
before any production deployment.

**Auth middleware is registered but unconfigured.** `UseAuthentication()` and
`UseAuthorization()` are present in the middleware pipeline to establish correct ordering,
but no auth schemes are configured. All endpoints are effectively public at this stage.
Feature 2 (Dev Auth Stub) and Phase 2 (JWT) will activate these.

**Storage variables present but unused.** The `Storage__*` environment variables are
declared and passed through Docker Compose, but no S3 client or storage service is wired
in code. File storage is a Phase 2 concern.

---

## Notes: Brief vs implementation

The implementation matches the Feature Brief with one addition: the Docker Compose `api`
service definition includes `Storage__*` environment variables (`Storage__ServiceUrl`,
`Storage__AccessKey`, `Storage__SecretKey`, `Storage__BucketName`, `Storage__Region`)
that were listed in the `.env.example` section of the brief but not explicitly called out
in the Docker Compose service definition section. The Backend Agent included them in the
compose file for completeness so the API container receives them when storage is wired in
Phase 2. This is a forward-compatible addition, not a scope deviation.
