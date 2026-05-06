// CreateWorkflowCommandHandlerTests — unit tests for CreateWorkflowCommandHandler.
//
// Covered behaviours:
//   1. Happy path — workflow and tasks are created; Result.Ok with full WorkflowDto returned
//   2. Empty task list — workflow is created with no tasks; Result.Ok with empty Tasks
//   3. Tasks are mapped with correct OrderIndex values from the command
//   4. WorkspaceId is sourced from ICurrentUserService, not from the command
//   5. SaveChangesAsync is called once per invocation
//   6. AddRangeAsync is not called when the task list is empty
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using Moq;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Features.Workflows.Commands.CreateWorkflow;
using StackFlow.Application.Features.Workflows.DTOs;
using StackFlow.Domain.Enums;
using StackFlow.Domain.Models;

namespace StackFlow.UnitTests.Workflows.Commands;

public class CreateWorkflowCommandHandlerTests
{
    // ── Shared fixture data ───────────────────────────────────────────────────

    private static readonly Guid WorkspaceId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private const string UserEmail = "test@stackflow.local";

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds mocks and returns the handler under test.
    /// Out parameters give callers access to mocks for verification.
    /// </summary>
    private static CreateWorkflowCommandHandler BuildHandler(
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

        return new CreateWorkflowCommandHandler(
            repoMock.Object,
            taskRepoMock.Object,
            uowMock.Object,
            currentUserMock.Object);
    }

    /// <summary>
    /// Creates a minimal valid CreateWorkflowTaskDto.
    /// </summary>
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

    // ── Test 1: Happy path — returns Result.Ok ────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommandWithTasks_ReturnsResultOk()
    {
        // Arrange
        var handler = BuildHandler(out _, out _, out _, out _);
        var command = new CreateWorkflowCommand(
            Name: "Onboarding",
            Description: "New hire process",
            Category: "HR",
            Tasks: [MakeTaskDto(0), MakeTaskDto(1)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    // ── Test 2: Returned DTO has correct field values ─────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_ReturnedDtoMatchesCommandFields()
    {
        // Arrange
        var handler = BuildHandler(out _, out _, out _, out _);
        var command = new CreateWorkflowCommand(
            Name: "Onboarding",
            Description: "New hire process",
            Category: "HR",
            Tasks: [MakeTaskDto(0)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var dto = result.Value;
        Assert.Equal("Onboarding", dto.Name);
        Assert.Equal("New hire process", dto.Description);
        Assert.Equal("HR", dto.Category);
        Assert.True(dto.IsActive, "Newly created workflow must be IsActive = true");
        Assert.Equal(WorkspaceId.ToString(), dto.WorkspaceId);
    }

    // ── Test 3: WorkspaceId sourced from ICurrentUserService ─────────────────

    [Fact]
    public async Task Handle_ValidCommand_WorkspaceIdComesFromCurrentUserService()
    {
        // Arrange
        var differentWorkspaceId = Guid.NewGuid();
        var handler = BuildHandler(out _, out _, out _, out var currentUserMock);
        currentUserMock.Setup(u => u.WorkspaceId).Returns(differentWorkspaceId);

        var command = new CreateWorkflowCommand("Test", null, null, []);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — WorkspaceId comes from the service, not hardcoded
        Assert.Equal(differentWorkspaceId.ToString(), result.Value.WorkspaceId);
    }

    // ── Test 4: Tasks are created with correct OrderIndex values ─────────────

    [Fact]
    public async Task Handle_ValidCommandWithMultipleTasks_TasksHaveCorrectOrderIndex()
    {
        // Arrange
        var handler = BuildHandler(out _, out _, out _, out _);
        var command = new CreateWorkflowCommand(
            Name: "Workflow",
            Description: null,
            Category: null,
            Tasks: [MakeTaskDto(0), MakeTaskDto(1), MakeTaskDto(2)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — OrderIndex is preserved from the command
        var taskDtos = result.Value.Tasks.OrderBy(t => t.OrderIndex).ToList();
        Assert.Equal(3, taskDtos.Count);
        Assert.Equal(0, taskDtos[0].OrderIndex);
        Assert.Equal(1, taskDtos[1].OrderIndex);
        Assert.Equal(2, taskDtos[2].OrderIndex);
    }

    // ── Test 5: Empty task list — workflow is created, no tasks ──────────────

    [Fact]
    public async Task Handle_CommandWithNoTasks_ReturnsOkWithEmptyTasksList()
    {
        // Arrange
        var handler = BuildHandler(out _, out _, out var uowMock, out _);
        var command = new CreateWorkflowCommand("Empty Workflow", null, null, []);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Tasks);
    }

    // ── Test 6: AddRangeAsync not called when task list is empty ─────────────

    [Fact]
    public async Task Handle_CommandWithNoTasks_DoesNotCallAddRangeAsync()
    {
        // Arrange
        var handler = BuildHandler(out _, out var taskRepoMock, out _, out _);
        var command = new CreateWorkflowCommand("Empty Workflow", null, null, []);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — AddRangeAsync must not be called for an empty task list
        taskRepoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<WorkflowTask>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── Test 7: AddRangeAsync called when task list is non-empty ─────────────

    [Fact]
    public async Task Handle_CommandWithTasks_CallsAddRangeAsyncOnce()
    {
        // Arrange
        var handler = BuildHandler(out _, out var taskRepoMock, out _, out _);
        var command = new CreateWorkflowCommand("Workflow", null, null, [MakeTaskDto(0)]);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        taskRepoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<WorkflowTask>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Test 8: SaveChangesAsync is called exactly once ───────────────────────

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveChangesAsyncOnce()
    {
        // Arrange
        var handler = BuildHandler(out _, out _, out var uowMock, out _);
        var command = new CreateWorkflowCommand("Workflow", null, null, [MakeTaskDto(0)]);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — exactly one save per handler call
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Test 9: AddAsync called on the workflow repository ───────────────────

    [Fact]
    public async Task Handle_ValidCommand_CallsWorkflowRepositoryAddAsync()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _, out _);
        var command = new CreateWorkflowCommand("Workflow", null, null, []);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repoMock.Verify(
            r => r.AddAsync(It.IsAny<Workflow>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Test 10: Returned DTO has a non-empty Id ──────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_ReturnedDtoHasNonEmptyId()
    {
        // Arrange
        var handler = BuildHandler(out _, out _, out _, out _);
        var command = new CreateWorkflowCommand("Workflow", null, null, []);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — the server generates a new Guid; it must not be the empty Guid
        Assert.False(string.IsNullOrEmpty(result.Value.Id));
        Assert.NotEqual(Guid.Empty.ToString(), result.Value.Id);
    }
}
