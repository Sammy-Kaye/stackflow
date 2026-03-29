using StackFlow.Domain.Enums;

namespace StackFlow.Domain.Models;

// A live instance of one WorkflowTask step within a running WorkflowState.
// Created for each WorkflowTask when a WorkflowState is spawned.
public class WorkflowTaskState
{
    public Guid Id { get; set; }

    // FK to the parent WorkflowState (the running workflow instance).
    public Guid WorkflowStateId { get; set; }

    // FK to the WorkflowTask template step this state was created from.
    public Guid WorkflowTaskId { get; set; }

    public WorkflowTaskStatus Status { get; set; }

    // The email address of the person assigned to this task at spawn time.
    public string AssignedToEmail { get; set; } = string.Empty;

    // UserId of the assignee, if they are a registered internal user.
    // Null for external contributors (who use token-based access instead).
    public Guid? AssignedToUserId { get; set; }

    // Calculated at spawn time from WorkflowTask.DueAtOffsetDays.
    // Never hardcoded — always derived.
    public DateTime? DueDate { get; set; }

    // Token sent to external contributors so they can complete this task without logging in.
    // Phase 2 — stored hashed, expires after 7 days, single-use.
    public string? CompletionToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsTokenUsed { get; set; }

    // Populated by the assignee when completing or declining the task.
    public string? CompletionNotes { get; set; }
    public string? DeclineReason { get; set; }

    public Priority Priority { get; set; }

    // Navigation properties — EF uses these; WorkflowTaskState itself has no EF dependency.
    public WorkflowState WorkflowState { get; set; } = null!;
    public WorkflowTask WorkflowTask { get; set; } = null!;
    public ICollection<WorkflowTaskAudit> Audits { get; set; } = new List<WorkflowTaskAudit>();
}
