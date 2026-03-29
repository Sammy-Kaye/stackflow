using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Infrastructure.Persistence;
using StackFlow.Infrastructure.Persistence.Repositories;

namespace StackFlow.Infrastructure;

// Registers all Infrastructure-layer services into the DI container.
// Called once from Program.cs: builder.Services.AddInfrastructure(builder.Configuration)
//
// What is registered here:
//   - AppDbContext (EF Core + Npgsql, reads ConnectionStrings:DefaultConnection)
//
// What is NOT registered here (added in later features):
//   - RabbitMQ IEventBus (Phase 2)
//   - MailKit IEmailService (Phase 2)
//   - S3 IFileStorage (Phase 2)
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core + PostgreSQL ──────────────────────────────────────────────
        // Connection string is read from ConnectionStrings:DefaultConnection.
        // In production, this is set via the ConnectionStrings__DefaultConnection
        // environment variable (double underscore = colon in .NET configuration).
        // Never hardcode a connection string.
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing. " +
                "Set the ConnectionStrings__DefaultConnection environment variable.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // ── Repositories (Feature 4) ──────────────────────────────────────────
        // Scoped lifetime: one instance per HTTP request, shared within the same
        // request so all repositories share the same DbContext and participate in
        // the same implicit EF Core transaction.
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<IWorkflowTaskRepository, WorkflowTaskRepository>();
        services.AddScoped<IWorkflowStateRepository, WorkflowStateRepository>();
        services.AddScoped<IWorkflowTaskStateRepository, WorkflowTaskStateRepository>();
        services.AddScoped<IWorkflowAuditRepository, WorkflowAuditRepository>();
        services.AddScoped<IWorkflowTaskAuditRepository, WorkflowTaskAuditRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
