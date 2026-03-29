using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackFlow.Domain.Models;

namespace StackFlow.Infrastructure.Persistence.Configurations;

public class WorkflowStateConfiguration : IEntityTypeConfiguration<WorkflowState>
{
    public void Configure(EntityTypeBuilder<WorkflowState> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Status)
            .IsRequired();

        builder.Property(s => s.ContextType)
            .IsRequired();

        builder.Property(s => s.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.StartedAt)
            .IsRequired()
            .HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        builder.Property(s => s.CompletedAt)
            .HasConversion(
                v => v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        builder.Property(s => s.CancelledAt)
            .HasConversion(
                v => v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        // A WorkflowState references the Workflow template it was spawned from.
        // Restrict delete — a workflow template cannot be deleted while live instances exist.
        builder.HasOne(s => s.Workflow)
            .WithMany(w => w.States)
            .HasForeignKey(s => s.WorkflowId)
            .OnDelete(DeleteBehavior.Restrict);

        // A WorkflowState belongs to a workspace.
        // Restrict delete — a workspace cannot be deleted while active workflow instances exist.
        builder.HasOne(s => s.Workspace)
            .WithMany(w => w.WorkflowStates)
            .HasForeignKey(s => s.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("WorkflowStates");
    }
}
