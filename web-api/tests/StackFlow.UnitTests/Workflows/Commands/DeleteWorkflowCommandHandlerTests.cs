// DeleteWorkflowCommandHandlerTests — unit tests for DeleteWorkflowCommandHandler.
//
// Covered behaviours:
//   1. Happy path — workflow deleted; Result.Ok (no value) returned
//   2. Workflow not found → Result.Fail("Workflow not found")
//   3. Cross-workspace access → Result.Fail("Workflow not found")
//      (enumeration prevention: same message as missing)
//   4. Global template protection → Result.Fail("Global starter templates cannot be deleted")
//      This is only reachable when the caller's workspace IS the GlobalWorkspaceId.
//      In Phase 1 no user owns GlobalWorkspaceId; this guard is belt-and-suspenders.
//   5. DeleteAsync called on the repository on success
//   6. SaveChangesAsync called exactly once on success
//   7. SaveChangesAsync not called on any failure path
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using Moq;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Features.Workflows.Commands.DeleteWorkflow;
using StackFlow.Domain.Constants;
using StackFlow.Domain.Models;

namespace StackFlow.UnitTests.Workflows.Commands;

public class DeleteWorkflowCommandHandlerTests
{
    // ── Shared fixture data ───────────────────────────────────────────────────

    private static readonly Guid WorkspaceId = Guid.NewGuid();
    private static readonly Guid OtherWorkspaceId = Guid.NewGuid();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DeleteWorkflowCommandHandler BuildHandler(
        out Mock<IWorkflowRepository> repoMock,
        out Mock<IUnitOfWork> uowMock,
        out Mock<ICurrentUserService> currentUserMock,
        Guid? callerWorkspaceId = null)
    {
        repoMock = new Mock<IWorkflowRepository>();
        uowMock = new Mock<IUnitOfWork>();
        currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock.Setup(u => u.WorkspaceId).Returns(callerWorkspaceId ?? WorkspaceId);

        return new DeleteWorkflowCommandHandler(
            repoMock.Object,
            uowMock.Object,
            currentUserMock.Object);
    }

    private static Workflow BuildWorkflow(Guid? workspaceId = null) => new()
    {
        Id = Guid.NewGuid(),
        WorkspaceId = workspaceId ?? WorkspaceId,
        Name = "Onboarding",
        Description = string.Empty,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    // ── Test 1: Happy path — returns Result.Ok ────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_WorkflowExists_ReturnsResultOk()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _);
        var workflow = BuildWorkflow();

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);

        var command = new DeleteWorkflowCommand(workflow.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    // ── Test 2: DeleteAsync called on success ─────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryDeleteAsync()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _);
        var workflow = BuildWorkflow();

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);

        var command = new DeleteWorkflowCommand(workflow.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repoMock.Verify(
            r => r.DeleteAsync(workflow, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Test 3: SaveChangesAsync called once on success ───────────────────────

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveChangesAsyncOnce()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var uowMock, out _);
        var workflow = BuildWorkflow();

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);

        var command = new DeleteWorkflowCommand(workflow.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Test 4: Workflow not found → Result.Fail ──────────────────────────────

    [Fact]
    public async Task Handle_WorkflowNotFound_ReturnsResultFail_WithNotFoundMessage()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _);
        var unknownId = Guid.NewGuid();

        repoMock.Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Workflow?)null);

        var command = new DeleteWorkflowCommand(unknownId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Workflow not found", result.Error);
    }

    // ── Test 5: Cross-workspace → Result.Fail with same "not found" message ───

    [Fact]
    public async Task Handle_WorkflowBelongsToDifferentWorkspace_ReturnsResultFail_WithNotFoundMessage()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _);
        // Workflow exists but belongs to a different workspace than the caller
        var workflow = BuildWorkflow(workspaceId: OtherWorkspaceId);

        repoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(workflow);

        var command = new DeleteWorkflowCommand(workflow.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — "not found" is returned, not "forbidden" (prevents enumeration)
        Assert.False(result.IsSuccess);
        Assert.Equal("Workflow not found", result.Error);
    }

    // ── Test 6: Global template protection ───────────────────────────────────
    // The handler first checks workspace ownership. The global template check is only
    // reachable when the caller's WorkspaceId matches the workflow's WorkspaceId, which
    // means the caller's workspace IS GlobalWorkspaceId. We simulate this here.

    [Fact]
    public async Task Handle_GlobalTemplate_WithCallerInGlobalWorkspace_ReturnsResultFail_WithGlobalTemplateMessage()
    {
        // Arrange — caller's workspace IS the GlobalWorkspaceId (simulates belt-and-suspenders guard)
        var handler = BuildHandler(
            out var repoMock,
            out _,
            out _,
            callerWorkspaceId: WellKnownIds.GlobalWorkspaceId);

        var globalWorkflow = BuildWorkflow(workspaceId: WellKnownIds.GlobalWorkspaceId);

        repoMock.Setup(r => r.GetByIdAsync(globalWorkflow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(globalWorkflow);

        var command = new DeleteWorkflowCommand(globalWorkflow.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Global starter templates cannot be deleted", result.Error);
    }

    // ── Test 7: SaveChangesAsync not called on failure ────────────────────────

    [Fact]
    public async Task Handle_WorkflowNotFound_DoesNotCallSaveChangesAsync()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out var uowMock, out _);
        var unknownId = Guid.NewGuid();

        repoMock.Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Workflow?)null);

        var command = new DeleteWorkflowCommand(unknownId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Test 8: DeleteAsync not called on failure ─────────────────────────────

    [Fact]
    public async Task Handle_WorkflowNotFound_DoesNotCallRepositoryDeleteAsync()
    {
        // Arrange
        var handler = BuildHandler(out var repoMock, out _, out _);
        var unknownId = Guid.NewGuid();

        repoMock.Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Workflow?)null);

        var command = new DeleteWorkflowCommand(unknownId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repoMock.Verify(
            r => r.DeleteAsync(It.IsAny<Workflow>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
