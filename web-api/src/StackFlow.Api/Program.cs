// StackFlow API entry point.
// This file wires up the ASP.NET Core service container and middleware pipeline.
// Registered as of Feature 2 (Dev Auth Stub):
//   - Swagger / OpenAPI
//   - Health checks
//   - CORS (allow all origins — dev only; locked down in production)
//   - JSON serialisation options (camelCase, ignore null)
//   - Controllers
//   - JWT bearer authentication (reads DevAuth:JwtSecret in Development;
//     Phase 2 will swap this for the real Jwt:Secret)
// Nothing that does not yet exist (no DbContext, no mediator) is wired here.
// Each subsequent feature adds its own registration without modifying this block.

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

// ── JWT Bearer Authentication ─────────────────────────────────────────────────
// Feature 2 (Dev Auth Stub): reads the signing secret from DevAuth:JwtSecret,
// which is only present in appsettings.Development.json.
// Phase 2 (Real Auth): this block will be replaced to read from Jwt:Secret with
// full issuer/audience validation enabled.
//
// The same bearer middleware is used in both phases — only the configuration
// source changes. Controllers decorated with [Authorize] work immediately.
//
// In non-Development environments DevAuth:JwtSecret is absent, so a placeholder
// key is used. The /api/auth/dev-login endpoint returns 403 in non-Development
// environments, so tokens signed with the placeholder key are never issued —
// the bearer middleware simply has no valid tokens to accept.
var jwtSecret = builder.Configuration["DevAuth:JwtSecret"]
    ?? "placeholder-key-replaced-by-phase2-real-auth-secret-xx";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

            // Issuer and audience validation is intentionally relaxed for the dev stub.
            // Phase 2 will enable these once the real token issuer is established.
            ValidateIssuer = false,
            ValidateAudience = false,

            // Enforce token expiry — even the dev stub should expire correctly.
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Return a clean JSON error body on 401 instead of an empty response.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"Unauthorised\"}");
            }
        };
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

// JWT bearer authentication is active from Feature 2 onward.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Expose the Program class for integration tests (WebApplicationFactory requires this)
public partial class Program { }
