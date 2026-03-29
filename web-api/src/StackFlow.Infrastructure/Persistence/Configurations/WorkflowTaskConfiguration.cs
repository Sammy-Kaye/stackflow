using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackFlow.Domain.Models;

namespace StackFlow.Infrastructure.Persistence.Configurations;

public class WorkflowTaskConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.AssigneeType)
            .IsRequired();

        builder.Property(t => t.DefaultAssignedToEmail)
            .HasMaxLength(256);

        builder.Property(t => t.OrderIndex)
            .IsRequired();

        builder.Property(t => t.DueAtOffsetDays)
            .IsRequired();

        builder.Property(t => t.NodeType)
            .IsRequired();

        builder.Property(t => t.ConditionConfig)
            .HasMaxLength(4000);

        // A task belongs to exactly one workflow template.
        // Cascade delete — deleting a workflow deletes all its task definitions.
        builder.HasOne(t => t.Workflow)
            .WithMany(w => w.Tasks)
            .HasForeignKey(t => t.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing FK for condition branches.
        // Restrict delete — a parent condition task cannot be deleted while child tasks exist.
        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.ChildTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("WorkflowTasks");
    }
}
