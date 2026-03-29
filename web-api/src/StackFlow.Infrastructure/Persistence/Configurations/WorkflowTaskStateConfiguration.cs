using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackFlow.Domain.Models;

namespace StackFlow.Infrastructure.Persistence.Configurations;

public class WorkflowTaskStateConfiguration : IEntityTypeConfiguration<WorkflowTaskState>
{
    public void Configure(EntityTypeBuilder<WorkflowTaskState> builder)
    {
        builder.HasKey(ts => ts.Id);

        builder.Property(ts => ts.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ts => ts.Status)
            .IsRequired();

        builder.Property(ts => ts.AssignedToEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ts => ts.DueDate)
            .HasConversion(
                v => v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        // CompletionToken is stored hashed in Phase 2. Max 500 chars to accommodate hashed values.
        builder.Property(ts => ts.CompletionToken)
            .HasMaxLength(500);

        builder.Property(ts => ts.TokenExpiresAt)
            .HasConversion(
                v => v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        builder.Property(ts => ts.IsTokenUsed)
            .IsRequired();

        builder.Property(ts => ts.CompletionNotes)
            .HasMaxLength(2000);

        builder.Property(ts => ts.DeclineReason)
            .HasMaxLength(2000);

        builder.Property(ts => ts.Priority)
            .IsRequired();

        // A WorkflowTaskState belongs to a WorkflowState (the parent running instance).
        // Cascade delete — deleting a workflow instance deletes all its task states.
        builder.HasOne(ts => ts.WorkflowState)
            .WithMany(s => s.TaskStates)
            .HasForeignKey(ts => ts.WorkflowStateId)
            .OnDelete(DeleteBehavior.Cascade);

        // A WorkflowTaskState references the WorkflowTask template step it was created from.
        // Restrict delete — a task template cannot be deleted while live task states reference it.
        builder.HasOne(ts => ts.WorkflowTask)
            .WithMany(t => t.States)
            .HasForeignKey(ts => ts.WorkflowTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("WorkflowTaskStates");
    }
}
