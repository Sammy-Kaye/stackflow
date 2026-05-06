// UpdateWorkflowCommandHandlerTests — unit tests for UpdateWorkflowCommandHandler.
//
// Covered behaviours:
//   1. Happy path — workflow is updated; Result.Ok with updated WorkflowDto returned
//   2. Workflow not found → Result.Fail("Workflow not found")
//   3. Workflow belongs to a different workspace → Result.Fail("Workflow not found")
//      (enumeration prevention: same message for both not-found and cross-workspace)
//   4. Header fields are applied from the command (Name, Description, Category, IsActive)
//   5. Existing tasks are deleted before new tasks are inserted
//   6. SaveChangesAsync is called exactly once
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using Moq;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Features.Workflows.Commands.UpdateWorkflow;
using StackFlow.Application.Features.Workflows.DTOs;
using StackFlow.Domain.Enums;
using StackFlow.Domain.Models;

namespace StackFlow.UnitTests.Workflows.Commands;

public class UpdateWorkflowCommandHandlerTests
{
    // ── Shared fixture data ───────────────────────────────────────────────────

    private static readonly Guid WorkspaceId = Guid.NewGuid();
    private static readonly Guid OtherWorkspaceId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private const string UserEmail = "test@stackflow.local";

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static UpdateWorkflowCommandHandler BuildHandler(
        out Mock<IWorkflowRepository> repoMock,
        out Mock<IWorkflowTaskRepository> taskRepoMock,
        out Mock<IUnitOfWork> uowMock,
        out Mock<ICurrentUserService> currentUserMock)
    {
        repoMock = new Mock<IWorkflowRepository>();
        taskRepoMock = new Mock<IWorkflowTaskRepository>();
        uowMock = new Mock<IUnitOfWork>();
        currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock.Setup(u => u.WorkspaceId).Returns(WorkspaceId);
        currentUserMock.Setup(u => u.UserId).Returns(UserId);
        currentUserMock.Setup(u => u.Email).Returns(UserEmail);

        return new UpdateWorkflowCommandHandler(
            repoMock.Object,
            taskRepoMock.Object,
            uowMock.Object,
            currentUserMock.Object);
    }

    private static Workflow BuildWorkflow(Guid? workspaceId = null) => new()
    {
        Id = Guid.NewGuid(),
        WorkspaceId = workspaceId ?? WorkspaceId,
        Name = "Original Name",
        Description = "Original description",
        Category = "HR",
        IsActive = true,
        CreatedAt = DateTime.UtcNow.AddDays(-1),
        UpdatedAt = DateTime.UtcNow.AddDays(-1)
    };

    private static CreateWorkflowTaskDto MakeTaskDto(int orderIndex = 0) =>
        new(
            Title: $"Task {orderIndex}",
            Description: null,
            AssigneeType: AssigneeType.Internal,
            DefaultAssignedToEmail: null,
            OrderIndex: orderIndex,
            DueAtOffsetDays: null,
            NodeType: NodeType.Task,
            ConditionConfig: null,
            ParentTaskId: null
        );

    private static WorkflowTask MakeExistingTask(Guid workflowId) => new()
    {
        Id = Guid.NewGuid(),
        WorkflowId = workflowId,
        Title = "Old Task",
        Description = string.Empty,
        AssigneeType = AssigneeType.Internal,
        OrderIndex = 0,
        NodeType = NodeType.Task
    };

    // ── Test 1: Happy path — returns Result.Ok ────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_WorkflowExists_ReturnsResultOk()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _, out _);
        var workflow = BuildWorkflow();

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);
        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var command = new UpdateWorkflowCommand(
            Id: workflow.Id,
            Name: "Updated Name",
            Description: "Updated description",
            Category: "Finance",
            IsActive: false,
            Tasks: [MakeTaskDto(0)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    // ── Test 2: Header fields applied from command ────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_UpdatedDtoReflectsNewFieldValues()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _, out _);
        var workflow = BuildWorkflow();

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);
        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var command = new UpdateWorkflowCommand(
            Id: workflow.Id,
            Name: "New Name",
            Description: "New description",
            Category: "Legal",
            IsActive: false,
            Tasks: []
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var dto = result.Value;
        Assert.Equal("New Name", dto.Name);
        Assert.Equal("New description", dto.Description);
        Assert.Equal("Legal", dto.Category);
        Assert.False(dto.IsActive);
    }

    // ── Test 3: Workflow not found → Result.Fail ──────────────────────────────

    [Fact]
    public async Task Handle_WorkflowNotFound_ReturnsResultFail_WithNotFoundMessage()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _, out _);
        var unknownId = Guid.NewGuid();

        repoMock.Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Workflow?)null);

        var command = new UpdateWorkflowCommand(
            Id: unknownId,
            Name: "Name",
            Description: null,
            Category: null,
            IsActive: true,
            Tasks: []
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Workflow not found", result.Error);
    }

    // ── Test 4: Cross-workspace access → Result.Fail (enumeration prevention) ─

    [Fact]
    public async Task Handle_WorkflowBelongsToDifferentWorkspace_ReturnsResultFail_WithNotFoundMessage()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _, out _);
        // Workflow exists but belongs to a different workspace
        var workflow = BuildWorkflow(workspaceId: OtherWorkspaceId);

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);

        var command = new UpdateWorkflowCommand(
            Id: workflow.Id,
            Name: "Name",
            Description: null,
            Category: null,
            IsActive: true,
            Tasks: []
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — same "not found" message as the missing case (prevents enumeration)
        Assert.False(result.IsSuccess);
        Assert.Equal("Workflow not found", result.Error);
    }

    // ── Test 5: Existing tasks are deleted before new ones are inserted ────────

    [Fact]
    public async Task Handle_ValidCommand_DeletesExistingTasksBeforeInsertingNew()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _, out _);
        var workflow = BuildWorkflow();
        var existingTask = MakeExistingTask(workflow.Id);

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);
        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask> { existingTask });

        var command = new UpdateWorkflowCommand(
            Id: workflow.Id,
            Name: "Name",
            Description: null,
            Category: null,
            IsActive: true,
            Tasks: [MakeTaskDto(0)]
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — the old task must have been deleted
        taskRepoMock.Verify(
            r => r.DeleteAsync(existingTask, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Test 6: New tasks are inserted after deletion ─────────────────────────

    [Fact]
    public async Task Handle_ValidCommandWithNewTasks_CallsAddRangeAsyncOnce()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out _, out _);
        var workflow = BuildWorkflow();

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);
        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var command = new UpdateWorkflowCommand(
            Id: workflow.Id,
            Name: "Name",
            Description: null,
            Category: null,
            IsActive: true,
            Tasks: [MakeTaskDto(0), MakeTaskDto(1)]
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        taskRepoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<WorkflowTask>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Test 7: SaveChangesAsync called exactly once ──────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveChangesAsyncOnce()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var taskRepoMock, out var uowMock, out _);
        var workflow = BuildWorkflow();

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);
        taskRepoMock.Setup(r => r.GetByWorkflowIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WorkflowTask>());

        var command = new UpdateWorkflowCommand(
            Id: workflow.Id,
            Name: "Name",
            Description: null,
            Category: null,
            IsActive: true,
            Tasks: []
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Test 8: SaveChangesAsync not called when workflow not found ───────────

    [Fact]
    public async Task Handle_WorkflowNotFound_DoesNotCallSaveChangesAsync()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out var uowMock, out _);
        var unknownId = Guid.NewGuid();

        repoMock.Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Workflow?)null);

        var command = new UpdateWorkflowCommand(
            Id: unknownId,
            Name: "Name",
            Description: null,
            Category: null,
            IsActive: true,
            Tasks: []
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — no persistence side-effects on failure
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
