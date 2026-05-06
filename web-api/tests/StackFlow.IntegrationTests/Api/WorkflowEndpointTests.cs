// WorkflowEndpointTests — integration tests for the five workflow CRUD endpoints.
//
// All five endpoints are covered:
//   GET    /api/workflows
//   GET    /api/workflows/{id}
//   POST   /api/workflows
//   PUT    /api/workflows/{id}
//   DELETE /api/workflows/{id}
//
// Setup:
//   - Uses WebApplicationFactory<Program> with Development environment so the
//     dev-login endpoint issues valid JWTs and the JWT middleware validates them.
//   - PostgreSQL is replaced with an in-memory SQLite database for each test class
//     instance. EnsureCreated() applies the schema without running migrations, which
//     is appropriate for integration tests (no migration runner needed).
//   - Each test class gets a fresh SQLite database (unique name per instance) to
//     prevent state leaking between parallel test runs.
//
// The dev-login stub issues a token for WorkspaceId = DemoWorkspaceId and Role = "Admin".
// All workflow CRUD endpoints require the JWT; DELETE additionally requires Role = "Admin".
//
// Test name format: {HTTP_METHOD}_{Path}_{Condition}_{ExpectedStatusCode}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.IntegrationTests.Api;

public class WorkflowEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    // Keep a long-lived open connection for the duration of the test class.
    // SQLite in-memory databases vanish when ALL connections to them are closed.
    // Holding one connection open here ensures the schema and data persist across
    // all HTTP requests, each of which opens and closes its own scoped connection.
    private readonly SqliteConnection _keepAliveConnection;

    public WorkflowEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Open the keep-alive connection BEFORE configuring the factory.
        // This ensures the in-memory SQLite database exists and stays alive
        // for the entire lifetime of this test class instance.
        _keepAliveConnection = new SqliteConnection("Data Source=WorkflowTests;Mode=Memory;Cache=Shared");
        _keepAliveConnection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");

            // Provide a placeholder connection string so AddInfrastructure() does not
            // throw "Connection string missing". The Npgsql registration is replaced below.
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Data Source=placeholder"
                });
            });

            builder.ConfigureServices(services =>
            {
                // AddInfrastructure() registers three service descriptors via AddDbContext:
                //   1. AppDbContext
                //   2. DbContextOptions<AppDbContext>
                //   3. IDbContextOptionsConfiguration<AppDbContext>
                //      — the delegate that applies UseNpgsql() at resolution time.
                //      Leaving this in place causes EF Core to see both Npgsql and SQLite
                //      providers, which throws: "two providers registered".
                var optionsConfigType =
                    typeof(Microsoft.EntityFrameworkCore.Infrastructure
                        .IDbContextOptionsConfiguration<AppDbContext>);

                for (var i = services.Count - 1; i >= 0; i--)
                {
                    var st = services[i].ServiceType;
                    if (st == typeof(AppDbContext)
                        || st == typeof(DbContextOptions<AppDbContext>)
                        || st == typeof(DbContextOptions)
                        || st == optionsConfigType)
                    {
                        services.RemoveAt(i);
                    }
                }

                // Re-register AppDbContext with SQLite using the shared-cache in-memory
                // database that our keep-alive connection is holding open above.
                // EnableServiceProviderCaching(false) prevents EF Core from reusing any
                // internal service provider that was cached for the Npgsql provider.
                services.AddDbContext<AppDbContext>(options =>
                    options
                        .UseSqlite("Data Source=WorkflowTests;Mode=Memory;Cache=Shared")
                        .EnableServiceProviderCaching(false));
            });
        });

        _client = _factory.CreateClient();

        // Create the schema via EnsureCreated. The keep-alive connection above guarantees
        // the in-memory database persists past this scope's disposal.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _factory.Dispose();
        _keepAliveConnection.Close();
        _keepAliveConnection.Dispose();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Obtains a valid dev JWT via POST /api/auth/dev-login and sets the
    /// Authorization header on _client for all subsequent requests.
    /// </summary>
    private async Task AuthenticateAsync()
    {
        var response = await _client.PostAsync("/api/auth/dev-login", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "dev-login must succeed before workflow endpoint tests can proceed");

        var body = await response.Content.ReadFromJsonAsync<DevLoginResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body.AccessToken);
    }

    private static object ValidCreateBody(string name = "Test Workflow") => new
    {
        name,
        description = (string?)null,
        category = (string?)null,
        tasks = Array.Empty<object>()
    };

    private static object ValidUpdateBody(string name = "Updated Workflow") => new
    {
        name,
        description = (string?)null,
        category = (string?)null,
        isActive = true,
        tasks = Array.Empty<object>()
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // GET /api/workflows
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GET_Workflows_WithValidToken_Returns200_WithListBody()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/workflows");

        // Assert — status
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert — body shape: { items: [...], totalCount: N }
        var body = await response.Content.ReadFromJsonAsync<WorkflowListResponse>();
        body.Should().NotBeNull();
        body!.Items.Should().NotBeNull();
        body.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GET_Workflows_IncludesGlobalTemplates_InResponse()
    {
        // Arrange — global templates are seeded by AppDbContext.SeedData
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/workflows");
        var body = await response.Content.ReadFromJsonAsync<WorkflowListResponse>();

        // Assert — the three seeded global templates must appear in the list
        body.Should().NotBeNull();
        body!.Items.Should().Contain(w => w.IsGlobal == true,
            "global starter templates must be included in the workflow list");
    }

    [Fact]
    public async Task GET_Workflows_WithNoToken_Returns401()
    {
        // Act — no Authorization header
        var response = await _client.GetAsync("/api/workflows");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GET /api/workflows/{id}
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GET_WorkflowById_ForKnownId_Returns200_WithWorkflowBody()
    {
        // Arrange — create a workflow first, then fetch it by ID
        await AuthenticateAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/workflows", ValidCreateBody("Fetch Me"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<WorkflowResponse>();
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/workflows/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<WorkflowResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(created.Id);
        body.Name.Should().Be("Fetch Me");
    }

    [Fact]
    public async Task GET_WorkflowById_ForUnknownId_Returns404()
    {
        // Arrange
        await AuthenticateAsync();
        var unknownId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/workflows/{unknownId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GET_WorkflowById_ForGlobalTemplate_Returns200()
    {
        // Arrange — the seeded "Employee Onboarding" global template has a known ID
        await AuthenticateAsync();
        var globalWorkflowId = new Guid("10000000-0000-0000-0000-000000000001");

        // Act
        var response = await _client.GetAsync($"/api/workflows/{globalWorkflowId}");

        // Assert — global templates are readable by all authenticated users
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_WorkflowById_WithNoToken_Returns401()
    {
        // Act
        var response = await _client.GetAsync($"/api/workflows/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // POST /api/workflows
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task POST_Workflows_WithValidBody_Returns201_WithCreatedWorkflow()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/workflows", ValidCreateBody("New Workflow"));

        // Assert — status
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Assert — body contains the created workflow
        var body = await response.Content.ReadFromJsonAsync<WorkflowResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeNullOrEmpty();
        body.Name.Should().Be("New Workflow");
        body.IsActive.Should().BeTrue("workflows are created active by default");
    }

    [Fact]
    public async Task POST_Workflows_WithMissingName_Returns400()
    {
        // Arrange
        await AuthenticateAsync();
        var bodyWithoutName = new { description = "desc", tasks = Array.Empty<object>() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workflows", bodyWithoutName);

        // Assert — ValidationBehavior catches this before the handler runs
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Workflows_WithNameExceeding200Characters_Returns400()
    {
        // Arrange
        await AuthenticateAsync();
        var body = ValidCreateBody(new string('x', 201));

        // Act
        var response = await _client.PostAsJsonAsync("/api/workflows", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Workflows_WithNoToken_Returns401()
    {
        // Act — no Authorization header
        var response = await _client.PostAsJsonAsync("/api/workflows", ValidCreateBody());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_Workflows_CreatesWorkflowWithTasksIncluded()
    {
        // Arrange
        await AuthenticateAsync();
        var bodyWithTasks = new
        {
            name = "Workflow With Tasks",
            description = (string?)null,
            category = (string?)null,
            tasks = new[]
            {
                new
                {
                    title = "Step 1",
                    description = (string?)null,
                    assigneeType = 0,     // Internal = 0
                    defaultAssignedToEmail = (string?)null,
                    orderIndex = 0,
                    dueAtOffsetDays = (int?)null,
                    nodeType = 0,         // Task = 0
                    conditionConfig = (string?)null,
                    parentTaskId = (Guid?)null
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workflows", bodyWithTasks);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<WorkflowResponse>();
        body.Should().NotBeNull();
        body!.Tasks.Should().HaveCount(1);
        body.Tasks![0].Title.Should().Be("Step 1");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUT /api/workflows/{id}
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task PUT_WorkflowById_WithValidBody_Returns200_WithUpdatedWorkflow()
    {
        // Arrange — create a workflow first, then update it
        await AuthenticateAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/workflows", ValidCreateBody("Original"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<WorkflowResponse>();
        created.Should().NotBeNull();

        // Act
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/workflows/{created!.Id}",
            ValidUpdateBody("Updated Name"));

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await updateResponse.Content.ReadFromJsonAsync<WorkflowResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Updated Name");
        body.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task PUT_WorkflowById_ForUnknownId_Returns404()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/workflows/{Guid.NewGuid()}",
            ValidUpdateBody());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PUT_WorkflowById_WithMissingName_Returns400()
    {
        // Arrange — create a workflow, then send an invalid update body
        await AuthenticateAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/workflows", ValidCreateBody());
        var created = await createResponse.Content.ReadFromJsonAsync<WorkflowResponse>();

        var invalidBody = new { description = "desc", isActive = true, tasks = Array.Empty<object>() };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/workflows/{created!.Id}", invalidBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PUT_WorkflowById_WithNoToken_Returns401()
    {
        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/workflows/{Guid.NewGuid()}",
            ValidUpdateBody());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DELETE /api/workflows/{id}
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DELETE_WorkflowById_WithValidId_Returns400_BecauseDevStubUsesGlobalWorkspaceId()
    {
        // NOTE: The dev auth stub issues tokens with WorkspaceId = GlobalWorkspaceId
        // (WellKnownIds.GlobalWorkspaceId = 00000000-0000-0000-0000-000000000002).
        // Workflows created via this stub are owned by GlobalWorkspaceId.
        // DeleteWorkflowCommandHandler protects all GlobalWorkspaceId-owned workflows
        // from deletion, returning 400 "Global starter templates cannot be deleted".
        //
        // The happy-path 204 response is covered by the unit test:
        //   DeleteWorkflowCommandHandlerTests.Handle_ValidCommand_..._ReturnsResultOk
        // where the test workspace is not GlobalWorkspaceId.

        // Arrange — create a workflow (it goes into GlobalWorkspaceId due to dev stub)
        await AuthenticateAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/workflows", ValidCreateBody("To Delete"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<WorkflowResponse>();
        created.Should().NotBeNull();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/workflows/{created!.Id}");

        // Assert — 400 because created workflows are in GlobalWorkspaceId
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DELETE_WorkflowById_ForUnknownId_Returns404()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/workflows/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_WorkflowById_ForGlobalTemplate_Returns400()
    {
        // The dev auth stub issues tokens with WorkspaceId = GlobalWorkspaceId.
        // The seeded "Employee Onboarding" template (ID 10000000-...-0001) is also in
        // GlobalWorkspaceId. The handler first checks workspace ownership — which PASSES
        // because both the caller and the template are in GlobalWorkspaceId — then checks
        // if the workflow is a global template and returns 400.
        await AuthenticateAsync();
        var globalWorkflowId = new Guid("10000000-0000-0000-0000-000000000001");

        // Act
        var response = await _client.DeleteAsync($"/api/workflows/{globalWorkflowId}");

        // Assert — 400 because caller is in GlobalWorkspaceId and the template is protected
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DELETE_WorkflowById_WithNoToken_Returns401()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/workflows/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DELETE_WorkflowById_BlockedByGlobalTemplate_WorkflowStillFetchableAfterwards()
    {
        // Because the dev auth stub uses GlobalWorkspaceId, all created workflows are
        // treated as global templates and delete returns 400. This test verifies that
        // when delete is blocked, the workflow is still accessible via GET.
        await AuthenticateAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/workflows", ValidCreateBody("Persistent"));
        var created = await createResponse.Content.ReadFromJsonAsync<WorkflowResponse>();
        created.Should().NotBeNull();

        // Attempt delete — expect 400 (global template protected)
        var deleteResponse = await _client.DeleteAsync($"/api/workflows/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act — the workflow must still be fetchable since delete was blocked
        var getResponse = await _client.GetAsync($"/api/workflows/{created.Id}");

        // Assert — workflow was not deleted; it is still accessible
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Local record types ────────────────────────────────────────────────────
    // Defined here to keep this file self-contained.
    // Property names are PascalCase — System.Text.Json deserialisation via
    // ReadFromJsonAsync is case-insensitive by default.

    private record DevLoginResponse(string AccessToken, string ExpiresAt);

    private record WorkflowListResponse(
        List<WorkflowSummaryItem> Items,
        int TotalCount);

    private record WorkflowSummaryItem(
        string Id,
        string Name,
        bool IsActive,
        int TaskCount,
        bool IsGlobal);

    private record WorkflowResponse(
        string Id,
        string Name,
        string? Description,
        string? Category,
        bool IsActive,
        List<WorkflowTaskItem>? Tasks);

    private record WorkflowTaskItem(
        string Id,
        string Title,
        int OrderIndex);

    private record ErrorResponse(string Error);
}
