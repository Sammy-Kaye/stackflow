// WorkflowMappingExtensions — extension methods that map Workflow and WorkflowTask
// domain entities to their DTO representations.
//
// All mapping lives here so the projection is defined once and consistently applied
// everywhere. If a DTO shape changes, there is one place to update.
//
// Description on Workflow uses empty-string → null coercion. The domain entity stores
// Description as a non-nullable string (empty string = no description), but the DTO
// represents it as nullable to match the API contract.

using StackFlow.Domain.Models;

namespace StackFlow.Application.Features.Workflows.DTOs;

public static class WorkflowMappingExtensions
{
    /// <summary>
    /// Projects a WorkflowTask entity to its DTO representation.
    /// AssigneeType and NodeType are returned as their string enum names (e.g. "Internal", "Task").
    /// </summary>
    public static WorkflowTaskDto ToTaskDto(this WorkflowTask task) => new(
        Id: task.Id.ToString(),
        WorkflowId: task.WorkflowId.ToString(),
        Title: task.Title,
        Description: string.IsNullOrEmpty(task.Description) ? null : task.Description,
        AssigneeType: task.AssigneeType.ToString(),
        DefaultAssignedToEmail: task.DefaultAssignedToEmail,
        OrderIndex: task.OrderIndex,
        DueAtOffsetDays: task.DueAtOffsetDays,
        NodeType: task.NodeType.ToString(),
        ConditionConfig: task.ConditionConfig,
        ParentTaskId: task.ParentTaskId?.ToString()
    );

    /// <summary>
    /// Projects a Workflow entity to its full DTO representation, including its task list.
    /// Tasks must be provided by the caller — the entity's Tasks navigation property is
    /// not used here to avoid unintended lazy-loading.
    /// </summary>
    public static WorkflowDto ToDto(this Workflow workflow, IReadOnlyList<WorkflowTaskDto> tasks) => new(
        Id: workflow.Id.ToString(),
        Name: workflow.Name,
        Description: string.IsNullOrEmpty(workflow.Description) ? null : workflow.Description,
        Category: workflow.Category,
        WorkspaceId: workflow.WorkspaceId.ToString(),
        IsActive: workflow.IsActive,
        CreatedAt: workflow.CreatedAt,
        UpdatedAt: workflow.UpdatedAt,
        Tasks: tasks
    );

    /// <summary>
    /// Projects a Workflow entity to its lightweight summary DTO for the list endpoint.
    /// TaskCount and IsGlobal are provided by the caller — they are computed by the handler.
    /// </summary>
    public static WorkflowSummaryDto ToSummaryDto(this Workflow workflow, int taskCount, bool isGlobal) => new(
        Id: workflow.Id.ToString(),
        Name: workflow.Name,
        Description: string.IsNullOrEmpty(workflow.Description) ? null : workflow.Description,
        Category: workflow.Category,
        IsActive: workflow.IsActive,
        TaskCount: taskCount,
        CreatedAt: workflow.CreatedAt,
        UpdatedAt: workflow.UpdatedAt,
        IsGlobal: isGlobal
    );
}
