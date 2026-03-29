using StackFlow.Domain.Enums;

namespace StackFlow.Domain.Models;

// A live running instance of a Workflow template.
// Created when a user "spawns" a workflow. Multiple WorkflowStates can exist for
// the same Workflow template (e.g. the same onboarding flow used for many employees).
public class WorkflowState
{
    public Guid Id { get; set; }

    // FK to the Workflow template this instance was spawned from.
    public Guid WorkflowId { get; set; }

    // FK to the workspace this instance belongs to.
    public Guid WorkspaceId { get; set; }

    public WorkflowStatus Status { get; set; }
    public ContextType ContextType { get; set; }

    // Groups multiple WorkflowState instances that were spawned together in a batch.
    // Null for Standalone instances.
    public Guid? BatchId { get; set; }

    // Human-readable reference number (e.g. "WF-2026-001"). Generated at spawn time.
    public string ReferenceNumber { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation properties — EF uses these; WorkflowState itself has no EF dependency.
    public Workflow Workflow { get; set; } = null!;
    public Workspace Workspace { get; set; } = null!;
    public ICollection<WorkflowTaskState> TaskStates { get; set; } = new List<WorkflowTaskState>();
    public ICollection<WorkflowAudit> Audits { get; set; } = new List<WorkflowAudit>();
}
