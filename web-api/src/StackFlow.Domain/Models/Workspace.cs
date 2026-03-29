namespace StackFlow.Domain.Models;

// A workspace is the top-level organisational container.
// All workflows, users, and workflow instances belong to a workspace.
public class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties — used by EF Core for queries; Workspace itself has no EF dependency.
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
    public ICollection<WorkflowState> WorkflowStates { get; set; } = new List<WorkflowState>();
}
