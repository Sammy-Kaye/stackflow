using StackFlow.Domain.Enums;

namespace StackFlow.Domain.Models;

// One step in a Workflow template.
// This is the blueprint for a single task node — not the live execution.
// The live execution copy is WorkflowTaskState (see WorkflowTaskState.cs).
public class WorkflowTask
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AssigneeType AssigneeType { get; set; }

    // Pre-filled assignee email for Internal tasks. Left null if the assignee is determined
    // at spawn time or if the task is External.
    public string? DefaultAssignedToEmail { get; set; }

    // Position of this step in the workflow sequence (0-based).
    public int OrderIndex { get; set; }

    // How many days after workflow spawn this task is due. Used to calculate DueDate on
    // WorkflowTaskState at spawn time. Never hardcode a date — always derive from this.
    public int DueAtOffsetDays { get; set; }

    public NodeType NodeType { get; set; }

    // Serialised JSON configuration for Condition nodes — stores branch rules.
    // Null for all other NodeType values.
    public string? ConditionConfig { get; set; }

    // Self-referencing FK — links a child branch task back to its parent Condition node.
    // Null for top-level (non-branched) tasks.
    public Guid? ParentTaskId { get; set; }

    // Navigation properties — EF uses these; WorkflowTask itself has no EF dependency.
    public Workflow Workflow { get; set; } = null!;
    public WorkflowTask? ParentTask { get; set; }
    public ICollection<WorkflowTask> ChildTasks { get; set; } = new List<WorkflowTask>();
    public ICollection<WorkflowTaskState> States { get; set; } = new List<WorkflowTaskState>();
}
