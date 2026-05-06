// GetWorkflowByIdQueryHandlerTests — unit tests for GetWorkflowByIdQueryHandler.
//
// Covered behaviours:
//   1. Happy path — workflow in caller's workspace → Result.Ok with full WorkflowDto
//   2. Global template (WorkspaceId == GlobalWorkspaceId) → Result.Ok (readable by all)
//   3. Workflow not found → Result.Fail("Workflow not found")
//   4. Cross-workspace access → Result.Fail("Workflow not found") (enumeration prevention)
//   5. Tasks are loaded and included in the returned DTO
//   6. Returned DTO includes the correct workflow fields
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using Moq;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Features.Workflows.Queries.GetWorkflowById;
using StackFlow.Domain.Constants;
using StackFlow.Domain.Enums;
using StackFlow.Domain.Models;

namespace StackFlow.UnitTests.Workflows.Queries;

public class GetWorkflowByIdQueryHandlerTests
{
    // ── Shared fixture data ───────────────────────────────────────────────────

    private static readonly Guid WorkspaceId = Guid.NewGuid();
    private static readonly Guid OtherWorkspaceId = Guid.NewGuid();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GetWorkflowByIdQueryHandler BuildHandler(
        out Mock<IWorkflowRepository> repoMock,
        out Mock<IWorkflowTaskRepository> taskRepoMock,
        out Mock<ICurrentUserService> currentUserMock,
        Guid? callerWorkspaceId = null)
    {
        repoMock = new Mock<IWorkflowRepository>();
        taskRepoMock = new Mock<IWorkflowTaskRepository>();
        currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock.Setup(u => u.WorkspaceId).Returns(callerWorkspaceId ?? WorkspaceId);

        return new GetWorkflowByIdQueryHandler(
            repoMock.Object,
            taskRepoMock.Object,
            currentUserMock.Object);
    }

    private static Workflow BuildWorkflow(Guid? workspaceId = null) => new()
    {
        Id = Guid.NewGuid(),
        WorkspaceId = workspaceId ?? WorkspaceId,
        Name = "Onboarding",
        Description = "New hire process",
        Category = "HR",
        IsActive = true,
        CreatedAt = DateTime.UtcNow.AddDays(-1),
        UpdatedAt = DateTime.UtcNow.AddDays(-1)
    };

    private static WorkflowTask BuildTask(Guid workflowId, int orderIndex = 0) => new()
    {
        Id = Guid.NewGuid(),
        WorkflowId = workflowId,
        Title = $"Task {orderIndex}",
        Description = string.Empty,
        AssigneeType = AssigneeType.Internal,
        OrderIndex = orderIndex,
        NodeType = NodeType.Task
    };

    // ── Test 1: Happy path — workspace-owned workflow ─────────────────────────

    [Fact]
    public async Task Handle_WorkflowInCallerWorkspace_ReturnsResultOk()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);
        var workflow = BuildWorkflow();

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);
        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var query = new GetWorkflowByIdQuery(workflow.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    // ── Test 2: Returned DTO has correct field values ─────────────────────────

    [Fact]
    public async Task Handle_WorkflowInCallerWorkspace_ReturnedDtoMatchesWorkflowEntity()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);
        var workflow = BuildWorkflow();

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);
        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var query = new GetWorkflowByIdQuery(workflow.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Value;
        Assert.Equal(workflow.Id.ToString(), dto.Id);
        Assert.Equal(workflow.Name, dto.Name);
        Assert.Equal(workflow.Description, dto.Description);
        Assert.Equal(workflow.Category, dto.Category);
        Assert.Equal(workflow.IsActive, dto.IsActive);
        Assert.Equal(workflow.WorkspaceId.ToString(), dto.WorkspaceId);
    }

    // ── Test 3: Tasks are loaded and included in the DTO ─────────────────────

    [Fact]
    public async Task Handle_WorkflowWithTasks_IncludesTasksInReturnedDto()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);
        var workflow = BuildWorkflow();
        var task1 = BuildTask(workflow.Id, 0);
        var task2 = BuildTask(workflow.Id, 1);

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);
        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask> { task1, task2 });

        var query = new GetWorkflowByIdQuery(workflow.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value.Tasks.Count);
    }

    // ── Test 4: Global template is readable by any workspace ─────────────────

    [Fact]
    public async Task Handle_GlobalTemplate_ReturnsResultOk_ForAnyCallerWorkspace()
    {
        // Arrange — caller is in a regular workspace, not the global workspace
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);
        var globalWorkflow = BuildWorkflow(workspaceId: WellKnownIds.GlobalWorkspaceId);

        repoMock.Setup(r => r.GetByIdAsync(globalWorkflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(globalWorkflow);
        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(globalWorkflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var query = new GetWorkflowByIdQuery(globalWorkflow.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — global templates are accessible to all authenticated users
        Assert.True(result.IsSuccess);
    }

    // ── Test 5: Workflow not found → Result.Fail ──────────────────────────────

    [Fact]
    public async Task Handle_WorkflowNotFound_ReturnsResultFail_WithNotFoundMessage()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _);
        var unknownId = Guid.NewGuid();

        repoMock.Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Workflow?)null);

        var query = new GetWorkflowByIdQuery(unknownId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Workflow not found", result.Error);
    }

    // ── Test 6: Cross-workspace access → Result.Fail (enumeration prevention) ─

    [Fact]
    public async Task Handle_WorkflowBelongsToDifferentWorkspace_ReturnsResultFail_WithNotFoundMessage()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _);
        // Workflow exists but belongs to a workspace the caller does not own
        var workflow = BuildWorkflow(workspaceId: OtherWorkspaceId);

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);

        var query = new GetWorkflowByIdQuery(workflow.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — 404 returned (not 403) to prevent workspace enumeration
        Assert.False(result.IsSuccess);
        Assert.Equal("Workflow not found", result.Error);
    }

    // ── Test 7: Task repository not queried when workflow is not found ────────

    [Fact]
    public async Task Handle_WorkflowNotFound_DoesNotQueryTaskRepository()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);
        var unknownId = Guid.NewGuid();

        repoMock.Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Workflow?)null);

        var query = new GetWorkflowByIdQuery(unknownId);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert — no unnecessary DB call on failure
        taskRepoMock.Verify(
            r => r.GetByWorkflowIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
