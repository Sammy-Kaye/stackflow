// GetWorkflowsQueryHandlerTests — unit tests for GetWorkflowsQueryHandler.
//
// Covered behaviours:
//   1. Returns both workspace-owned and global workflows in one response
//   2. Workspace workflows appear before global templates (ordering rule)
//   3. IsGlobal is false for workspace workflows, true for global workflows
//   4. TaskCount is included correctly for each summary item
//   5. Returns empty list when workspace has no workflows and no globals exist
//   6. WorkspaceId is sourced from ICurrentUserService, never from the query
//   7. TotalCount matches the number of items in the response
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using Moq;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Features.Workflows.Queries.GetWorkflows;
using StackFlow.Domain.Constants;
using StackFlow.Domain.Enums;
using StackFlow.Domain.Models;

namespace StackFlow.UnitTests.Workflows.Queries;

public class GetWorkflowsQueryHandlerTests
{
    // ── Shared fixture data ───────────────────────────────────────────────────

    private static readonly Guid WorkspaceId = Guid.NewGuid();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GetWorkflowsQueryHandler BuildHandler(
        out Mock<IWorkflowRepository> repoMock,
        out Mock<IWorkflowTaskRepository> taskRepoMock,
        out Mock<ICurrentUserService> currentUserMock)
    {
        repoMock = new Mock<IWorkflowRepository>();
        taskRepoMock = new Mock<IWorkflowTaskRepository>();
        currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock.Setup(u => u.WorkspaceId).Returns(WorkspaceId);

        return new GetWorkflowsQueryHandler(
            repoMock.Object,
            taskRepoMock.Object,
            currentUserMock.Object);
    }

    private static Workflow BuildWorkflow(Guid workspaceId, string name = "Workflow") => new()
    {
        Id = Guid.NewGuid(),
        WorkspaceId = workspaceId,
        Name = name,
        Description = string.Empty,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static WorkflowTask BuildTask(Guid workflowId) => new()
    {
        Id = Guid.NewGuid(),
        WorkflowId = workflowId,
        Title = "A task",
        Description = string.Empty,
        AssigneeType = AssigneeType.Internal,
        OrderIndex = 0,
        NodeType = NodeType.Task
    };

    // ── Test 1: Happy path — returns workspace and global workflows ───────────

    [Fact]
    public async Task Handle_Query_ReturnsWorkspaceAndGlobalWorkflows()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);

        var workspaceWorkflow = BuildWorkflow(WorkspaceId, "Workspace Workflow");
        var globalWorkflow = BuildWorkflow(WellKnownIds.GlobalWorkspaceId, "Global Template");

        repoMock.Setup(r => r.GetByWorkspaceAsync(WorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow> { workspaceWorkflow });
        repoMock.Setup(r => r.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow> { globalWorkflow });

        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var query = new GetWorkflowsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(2, result.Value.TotalCount);
    }

    // ── Test 2: Workspace workflows appear before global templates ────────────

    [Fact]
    public async Task Handle_Query_WorkspaceWorkflowsAppearBeforeGlobalTemplates()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);

        var workspaceWorkflow = BuildWorkflow(WorkspaceId, "Workspace Workflow");
        var globalWorkflow = BuildWorkflow(WellKnownIds.GlobalWorkspaceId, "Global Template");

        repoMock.Setup(r => r.GetByWorkspaceAsync(WorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow> { workspaceWorkflow });
        repoMock.Setup(r => r.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow> { globalWorkflow });

        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var query = new GetWorkflowsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — workspace workflow must be first
        Assert.Equal("Workspace Workflow", result.Value.Items[0].Name);
        Assert.Equal("Global Template", result.Value.Items[1].Name);
    }

    // ── Test 3: IsGlobal flag set correctly ───────────────────────────────────

    [Fact]
    public async Task Handle_Query_IsGlobalIsFalseForWorkspaceWorkflows()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);
        var workspaceWorkflow = BuildWorkflow(WorkspaceId);

        repoMock.Setup(r => r.GetByWorkspaceAsync(WorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow> { workspaceWorkflow });
        repoMock.Setup(r => r.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow>());

        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var query = new GetWorkflowsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Value.Items[0].IsGlobal);
    }

    [Fact]
    public async Task Handle_Query_IsGlobalIsTrueForGlobalTemplates()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);
        var globalWorkflow = BuildWorkflow(WellKnownIds.GlobalWorkspaceId);

        repoMock.Setup(r => r.GetByWorkspaceAsync(WorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow>());
        repoMock.Setup(r => r.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow> { globalWorkflow });

        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var query = new GetWorkflowsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Value.Items[0].IsGlobal);
    }

    // ── Test 4: TaskCount is included correctly ───────────────────────────────

    [Fact]
    public async Task Handle_Query_TaskCountReflectsActualTaskCount()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);
        var workflow = BuildWorkflow(WorkspaceId);
        var task1 = BuildTask(workflow.Id);
        var task2 = BuildTask(workflow.Id);

        repoMock.Setup(r => r.GetByWorkspaceAsync(WorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow> { workflow });
        repoMock.Setup(r => r.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow>());

        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask> { task1, task2 });

        var query = new GetWorkflowsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value.Items[0].TaskCount);
    }

    // ── Test 5: Empty workspace and no globals → empty list ───────────────────

    [Fact]
    public async Task Handle_Query_NoWorkflowsAnywhere_ReturnsEmptyList()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _);

        repoMock.Setup(r => r.GetByWorkspaceAsync(WorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow>());
        repoMock.Setup(r => r.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow>());

        var query = new GetWorkflowsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    // ── Test 6: TotalCount matches item count ─────────────────────────────────

    [Fact]
    public async Task Handle_Query_TotalCountMatchesItemsCount()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _);

        var w1 = BuildWorkflow(WorkspaceId, "W1");
        var w2 = BuildWorkflow(WorkspaceId, "W2");
        var g1 = BuildWorkflow(WellKnownIds.GlobalWorkspaceId, "G1");

        repoMock.Setup(r => r.GetByWorkspaceAsync(WorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow> { w1, w2 });
        repoMock.Setup(r => r.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow> { g1 });

        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var query = new GetWorkflowsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — TotalCount == Items.Count (no pagination in Phase 1)
        Assert.Equal(result.Value.Items.Count, result.Value.TotalCount);
        Assert.Equal(3, result.Value.TotalCount);
    }

    // ── Test 7: Correct workspace ID is queried ───────────────────────────────

    [Fact]
    public async Task Handle_Query_QueriesCorrectWorkspaceIdFromCurrentUserService()
    {
        // Arrange
        var specificWorkspaceId = Guid.NewGuid();
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out var currentUserMock);
        currentUserMock.Setup(u => u.WorkspaceId).Returns(specificWorkspaceId);

        repoMock.Setup(r => r.GetByWorkspaceAsync(specificWorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow>());
        repoMock.Setup(r => r.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Workflow>());

        var query = new GetWorkflowsQuery();

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert — the handler must have called the repo with the correct workspace
        repoMock.Verify(
            r => r.GetByWorkspaceAsync(specificWorkspaceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
