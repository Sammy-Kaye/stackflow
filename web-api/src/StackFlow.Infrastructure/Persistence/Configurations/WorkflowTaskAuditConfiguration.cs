using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackFlow.Domain.Models;

namespace StackFlow.Infrastructure.Persistence.Configurations;

public class WorkflowTaskAuditConfiguration : IEntityTypeConfiguration<WorkflowTaskAudit>
{
    public void Configure(EntityTypeBuilder<WorkflowTaskAudit> builder)
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

        // Audit records reference their parent WorkflowTaskState.
        // Restrict delete — a task state cannot be deleted while audit records exist.
        builder.HasOne(a => a.WorkflowTaskState)
            .WithMany(ts => ts.Audits)
            .HasForeignKey(a => a.WorkflowTaskStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("WorkflowTaskAudits");
    }
}
