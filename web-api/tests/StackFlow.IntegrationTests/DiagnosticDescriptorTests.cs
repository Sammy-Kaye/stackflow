using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackFlow.Infrastructure;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.IntegrationTests;

public class DiagnosticDescriptorTests
{
    [Fact]
    public void PrintNpgsqlDescriptors()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost"
            })
            .Build();

        StackFlow.Infrastructure.DependencyInjection.AddInfrastructure(services, config);

        var output = new System.Text.StringBuilder();
        output.AppendLine($"Total descriptors: {services.Count}");
        foreach (var d in services)
        {
            var st = d.ServiceType.FullName ?? "";
            var impl = d.ImplementationType?.FullName ?? "(factory/instance)";
            if (st.Contains("Provider", StringComparison.OrdinalIgnoreCase) ||
                impl.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                impl.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) ||
                st.Contains("AppDbContext", StringComparison.OrdinalIgnoreCase))
            {
                output.AppendLine($"ST: {st}");
                output.AppendLine($"  IMPL: {impl}");
                output.AppendLine($"  Lifetime: {d.Lifetime}");
            }
        }

        // This test always fails to print the output
        Assert.Fail(output.ToString());
    }
}
