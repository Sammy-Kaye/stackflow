using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackFlow.Domain.Models;

namespace StackFlow.Infrastructure.Persistence.Configurations;

public class WorkflowAuditConfiguration : IEntityTypeConfiguration<WorkflowAudit>
{
    public void Configure(EntityTypeBuilder<WorkflowAudit> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.ActorEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.OldValue)
            .HasMaxLength(4000);

        builder.Property(a => a.NewValue)
            .HasMaxLength(4000);

        builder.Property(a => a.Timestamp)
            .IsRequired()
            .HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Audit records reference their parent WorkflowState.
        // Restrict delete — a workflow instance cannot be deleted while audit records exist.
        builder.HasOne(a => a.WorkflowState)
            .WithMany(s => s.Audits)
            .HasForeignKey(a => a.WorkflowStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("WorkflowAudits");
    }
}
