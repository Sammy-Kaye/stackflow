using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.Infrastructure;

// Registers all Infrastructure-layer services into the DI container.
// Called once from Program.cs: builder.Services.AddInfrastructure(builder.Configuration)
//
// What is registered here:
//   - AppDbContext (EF Core + Npgsql, reads ConnectionStrings:DefaultConnection)
//
// What is NOT registered here (added in later features):
//   - Repository implementations (Feature 4)
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

        return services;
    }
}
