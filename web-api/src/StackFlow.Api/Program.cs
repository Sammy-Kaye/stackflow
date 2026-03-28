// StackFlow API entry point.
// This file wires up the ASP.NET Core service container and middleware pipeline.
// At this stage (Feature 1 — Project Scaffold) only the following are registered:
//   - Swagger / OpenAPI
//   - Health checks
//   - CORS (allow all origins — dev only; locked down in production)
//   - JSON serialisation options (camelCase, ignore null)
//   - Controllers
// Nothing that does not yet exist (no DbContext, no mediator, no auth) is wired here.
// Each subsequent feature adds its own registration without modifying this block.

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // camelCase property names in all JSON responses
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;

        // Omit null values from responses — cleaner payloads, no "field": null noise
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
// Available at http://localhost:5000/swagger in development.
// Swashbuckle generates the UI; Microsoft.AspNetCore.OpenApi generates the spec.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "StackFlow API",
        Version = "v1",
        Description = "Intelligent adaptive workflow process engine"
    });
});

// ── Health checks ─────────────────────────────────────────────────────────────
// Exposed at GET /health — always returns 200 while the process is running.
// Future features can add database checks, RabbitMQ checks, etc. here.
builder.Services.AddHealthChecks();

// ── CORS ──────────────────────────────────────────────────────────────────────
// Dev policy: allow all origins, methods, and headers.
// This is intentionally permissive for local development only.
// In production, replace AllowAnyOrigin with the specific frontend domain.
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
// Order matters — CORS must come before routing, routing before auth, auth before endpoints.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "StackFlow API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("DevCors");

app.UseRouting();

// Auth middleware is registered here for correctness — no schemes are configured yet.
// Feature 2 (Dev Auth Stub) and Phase 2 (JWT) will activate these.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Expose the Program class for integration tests (WebApplicationFactory requires this)
public partial class Program { }
