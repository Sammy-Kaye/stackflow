using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StackFlow.Domain.Constants;
using StackFlow.Domain.Enums;
using StackFlow.Domain.Models;

namespace StackFlow.Infrastructure.Persistence;

// The EF Core DbContext for StackFlow.
//
// Design decisions:
//   - OnModelCreating delegates all entity configuration to IEntityTypeConfiguration<T>
//     files in the Configurations/ folder. This keeps this file small and focused.
//   - Assembly scanning (ApplyConfigurationsFromAssembly) picks up all configuration
//     classes automatically — no manual registration required.
//   - Seed data is applied here via HasData because it is tightly coupled to the schema
//     shape (fixed Guids, fixed column values). All seed Guids and timestamps are fixed
//     so that migrations are deterministic and idempotent.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<WorkflowState> WorkflowStates => Set<WorkflowState>();
    public DbSet<WorkflowTaskState> WorkflowTaskStates => Set<WorkflowTaskState>();
    public DbSet<WorkflowAudit> WorkflowAudits => Set<WorkflowAudit>();
    public DbSet<WorkflowTaskAudit> WorkflowTaskAudits => Set<WorkflowTaskAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> classes found in this assembly.
        // Adding a new configuration file in Configurations/ is sufficient — no manual wiring needed.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        SeedData(modelBuilder);
    }

    // ── Seed data ─────────────────────────────────────────────────────────────
    // All Guids and timestamps are fixed for deterministic, idempotent migrations.
    // Do not use DateTime.UtcNow here — it would produce a different migration every run.
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // ── Workspaces ────────────────────────────────────────────────────────
        modelBuilder.Entity<Workspace>().HasData(
            new Workspace
            {
                Id = WellKnownIds.DemoWorkspaceId,
                Name = "Demo Workspace",
                CreatedAt = seedDate
            },
            new Workspace
            {
                Id = WellKnownIds.GlobalWorkspaceId,
                Name = "Global",
                CreatedAt = seedDate
            }
        );

        // ── Workflow templates (in the Global workspace) ──────────────────────
        // Fixed Guids so the migration is deterministic.
        var onboardingId = new Guid("10000000-0000-0000-0000-000000000001");
        var purchaseId   = new Guid("10000000-0000-0000-0000-000000000002");
        var offboardingId = new Guid("10000000-0000-0000-0000-000000000003");

        modelBuilder.Entity<Workflow>().HasData(
            new Workflow
            {
                Id = onboardingId,
                WorkspaceId = WellKnownIds.GlobalWorkspaceId,
                Name = "Employee Onboarding",
                Description = "Standard onboarding process for new employees.",
                Category = "HR",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Workflow
            {
                Id = purchaseId,
                WorkspaceId = WellKnownIds.GlobalWorkspaceId,
                Name = "Purchase Approval",
                Description = "Approval workflow for purchase requests.",
                Category = "Finance",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Workflow
            {
                Id = offboardingId,
                WorkspaceId = WellKnownIds.GlobalWorkspaceId,
                Name = "Client Offboarding",
                Description = "Structured process for offboarding departing clients.",
                Category = "Operations",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );

        // ── Employee Onboarding — 6 task nodes ───────────────────────────────
        modelBuilder.Entity<WorkflowTask>().HasData(
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000001"),
                WorkflowId = onboardingId,
                Title = "Send offer letter",
                Description = "Prepare and send the signed offer letter to the new employee.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 0,
                DueAtOffsetDays = 1,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000002"),
                WorkflowId = onboardingId,
                Title = "Set up workstation",
                Description = "Provision laptop, accounts, and access credentials for the new starter.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 1,
                DueAtOffsetDays = 3,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000003"),
                WorkflowId = onboardingId,
                Title = "Schedule orientation session",
                Description = "Book the orientation session and send calendar invites.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 2,
                DueAtOffsetDays = 3,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000004"),
                WorkflowId = onboardingId,
                Title = "Complete HR paperwork",
                Description = "Ensure all employment forms, tax declarations, and NDAs are signed.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 3,
                DueAtOffsetDays = 5,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000005"),
                WorkflowId = onboardingId,
                Title = "Assign buddy / mentor",
                Description = "Pair the new employee with an experienced team member.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 4,
                DueAtOffsetDays = 5,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000006"),
                WorkflowId = onboardingId,
                Title = "30-day check-in",
                Description = "Schedule and conduct a 30-day review meeting with the new starter.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 5,
                DueAtOffsetDays = 30,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            }
        );

        // ── Purchase Approval — 4 nodes (3x Task, 1x Approval) ───────────────
        modelBuilder.Entity<WorkflowTask>().HasData(
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000007"),
                WorkflowId = purchaseId,
                Title = "Submit purchase request",
                Description = "Complete the purchase request form with vendor details, cost, and business justification.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 0,
                DueAtOffsetDays = 1,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000008"),
                WorkflowId = purchaseId,
                Title = "Manager approval",
                Description = "Line manager reviews and approves or declines the purchase request.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 1,
                DueAtOffsetDays = 3,
                NodeType = NodeType.Approval,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000009"),
                WorkflowId = purchaseId,
                Title = "Raise purchase order",
                Description = "Finance team raises and issues the purchase order to the vendor.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 2,
                DueAtOffsetDays = 5,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000010"),
                WorkflowId = purchaseId,
                Title = "Confirm delivery and close",
                Description = "Verify goods or services received and mark the purchase order as complete.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 3,
                DueAtOffsetDays = 14,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            }
        );

        // ── Client Offboarding — 5 task nodes ────────────────────────────────
        modelBuilder.Entity<WorkflowTask>().HasData(
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000011"),
                WorkflowId = offboardingId,
                Title = "Send offboarding notification",
                Description = "Notify relevant internal teams of the client's departure date.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 0,
                DueAtOffsetDays = 1,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000012"),
                WorkflowId = offboardingId,
                Title = "Retrieve client assets",
                Description = "Collect any company equipment, access credentials, or materials from the client.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 1,
                DueAtOffsetDays = 3,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000013"),
                WorkflowId = offboardingId,
                Title = "Revoke system access",
                Description = "Remove client access from all internal systems and shared resources.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 2,
                DueAtOffsetDays = 3,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000014"),
                WorkflowId = offboardingId,
                Title = "Issue final invoice",
                Description = "Generate and send the final invoice for any outstanding services.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 3,
                DueAtOffsetDays = 5,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            },
            new WorkflowTask
            {
                Id = new Guid("20000000-0000-0000-0000-000000000015"),
                WorkflowId = offboardingId,
                Title = "Conduct exit interview",
                Description = "Schedule and conduct an exit interview to capture feedback.",
                AssigneeType = AssigneeType.Internal,
                DefaultAssignedToEmail = null,
                OrderIndex = 4,
                DueAtOffsetDays = 7,
                NodeType = NodeType.Task,
                ConditionConfig = null,
                ParentTaskId = null
            }
        );
    }
}
