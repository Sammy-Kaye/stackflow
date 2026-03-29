using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackFlow.Domain.Models;

namespace StackFlow.Infrastructure.Persistence.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedOnAdd();

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(w => w.Category)
            .HasMaxLength(200);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired()
            .HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        builder.Property(w => w.UpdatedAt)
            .IsRequired()
            .HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // A workflow belongs to exactly one workspace.
        // Restrict delete — a workspace cannot be deleted while workflows exist in it.
        builder.HasOne(w => w.Workspace)
            .WithMany(ws => ws.Workflows)
            .HasForeignKey(w => w.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Workflows");
    }
}
