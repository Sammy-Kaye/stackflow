namespace StackFlow.Domain.Models;

// A workflow template — the reusable blueprint.
// A workflow is NOT a live execution. It defines the steps, order, and assignees.
// Live executions are WorkflowState instances (see WorkflowState.cs).
public class Workflow
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Optional grouping label — e.g. "HR", "Finance", "Onboarding".
    public string? Category { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties — EF uses these; Workflow itself has no EF dependency.
    public Workspace Workspace { get; set; } = null!;
    public ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();
    public ICollection<WorkflowState> States { get; set; } = new List<WorkflowState>();
}
