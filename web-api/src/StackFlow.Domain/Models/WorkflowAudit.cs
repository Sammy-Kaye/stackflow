namespace StackFlow.Domain.Models;

// Immutable audit record for every state change on a WorkflowState.
// Written by handlers — never updated, never deleted.
// Reading the full audit log reconstructs the complete history of a workflow instance.
public class WorkflowAudit
{
    public Guid Id { get; set; }
    public Guid WorkflowStateId { get; set; }

    // Null when the action was performed by an automated system process (e.g. a scheduler).
    public Guid? ActorUserId { get; set; }

    public string ActorEmail { get; set; } = string.Empty;

    // Human-readable action name — e.g. "WorkflowStarted", "WorkflowCancelled".
    public string Action { get; set; } = string.Empty;

    // Serialised previous value of the changed field. Null if this is a create event.
    public string? OldValue { get; set; }

    // Serialised new value of the changed field.
    public string? NewValue { get; set; }

    public DateTime Timestamp { get; set; }

    // Navigation property — EF uses this; WorkflowAudit itself has no EF dependency.
    public WorkflowState WorkflowState { get; set; } = null!;
}
