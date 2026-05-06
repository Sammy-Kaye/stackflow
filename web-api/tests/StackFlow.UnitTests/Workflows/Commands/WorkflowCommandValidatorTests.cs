// WorkflowCommandValidatorTests — unit tests for all three workflow command validators.
//
// Validators covered:
//   - CreateWorkflowCommandValidator
//   - UpdateWorkflowCommandValidator
//   - DeleteWorkflowCommandValidator
//
// Each validator is tested against:
//   - Valid input (no errors expected)
//   - Each individual rule that can fail (exact error message asserted)
//
// Validators run synchronously via .Validate() rather than .ValidateAsync()
// because there are no async rules in these validators.
//
// Test name format: {Validator}_{Field}_{Condition}_{ExpectedResult}

using FluentValidation.TestHelper;
using StackFlow.Application.Features.Workflows.Commands.CreateWorkflow;
using StackFlow.Application.Features.Workflows.Commands.DeleteWorkflow;
using StackFlow.Application.Features.Workflows.Commands.UpdateWorkflow;
using StackFlow.Application.Features.Workflows.DTOs;
using StackFlow.Domain.Enums;

namespace StackFlow.UnitTests.Workflows.Commands;

public class WorkflowCommandValidatorTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CreateWorkflowCommandValidator
    // ═══════════════════════════════════════════════════════════════════════════

    private static readonly CreateWorkflowCommandValidator CreateValidator = new();

    private static CreateWorkflowTaskDto ValidTaskDto(int orderIndex = 0) =>
        new(
            Title: "Task Title",
            Description: null,
            AssigneeType: AssigneeType.Internal,
            DefaultAssignedToEmail: null,
            OrderIndex: orderIndex,
            DueAtOffsetDays: null,
            NodeType: NodeType.Task,
            ConditionConfig: null,
            ParentTaskId: null
        );

    private static CreateWorkflowCommand ValidCreateCommand() =>
        new(
            Name: "Valid Workflow",
            Description: null,
            Category: null,
            Tasks: [ValidTaskDto()]
        );

    // ── CreateWorkflow: valid input ───────────────────────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_ValidCommand_NoValidationErrors()
    {
        var result = CreateValidator.TestValidate(ValidCreateCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateWorkflowCommandValidator_EmptyTaskList_NoValidationErrors()
    {
        // An empty task list is valid — tasks are added later via the builder
        var command = ValidCreateCommand() with { Tasks = [] };
        var result = CreateValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── CreateWorkflow: Name ──────────────────────────────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_Name_WhenEmpty_FailsWithRequiredMessage()
    {
        var command = ValidCreateCommand() with { Name = "" };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void CreateWorkflowCommandValidator_Name_WhenExceeds200Characters_FailsWithMaxLengthMessage()
    {
        var command = ValidCreateCommand() with { Name = new string('x', 201) };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public void CreateWorkflowCommandValidator_Name_AtExactly200Characters_PassesValidation()
    {
        var command = ValidCreateCommand() with { Name = new string('x', 200) };
        var result = CreateValidator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ── CreateWorkflow: Description ───────────────────────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_Description_WhenNull_NoValidationError()
    {
        var command = ValidCreateCommand() with { Description = null };
        var result = CreateValidator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void CreateWorkflowCommandValidator_Description_WhenExceeds2000Characters_FailsWithMaxLengthMessage()
    {
        var command = ValidCreateCommand() with { Description = new string('x', 2001) };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 2000 characters.");
    }

    // ── CreateWorkflow: Category ──────────────────────────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_Category_WhenNull_NoValidationError()
    {
        var command = ValidCreateCommand() with { Category = null };
        var result = CreateValidator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public void CreateWorkflowCommandValidator_Category_WhenExceeds100Characters_FailsWithMaxLengthMessage()
    {
        var command = ValidCreateCommand() with { Category = new string('x', 101) };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Category)
              .WithErrorMessage("Category must not exceed 100 characters.");
    }

    // ── CreateWorkflow: Tasks collection ─────────────────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_Tasks_WhenNull_FailsWithNotNullMessage()
    {
        var command = ValidCreateCommand() with { Tasks = null! };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Tasks)
              .WithErrorMessage("Tasks must not be null.");
    }

    // ── CreateWorkflow: Task title ────────────────────────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_TaskTitle_WhenEmpty_FailsWithRequiredMessage()
    {
        var badTask = ValidTaskDto() with { Title = "" };
        var command = ValidCreateCommand() with { Tasks = [badTask] };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Tasks[0].Title")
              .WithErrorMessage("Task title is required.");
    }

    [Fact]
    public void CreateWorkflowCommandValidator_TaskTitle_WhenExceeds200Characters_FailsWithMaxLengthMessage()
    {
        var badTask = ValidTaskDto() with { Title = new string('x', 201) };
        var command = ValidCreateCommand() with { Tasks = [badTask] };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Tasks[0].Title")
              .WithErrorMessage("Task title must not exceed 200 characters.");
    }

    // ── CreateWorkflow: Task OrderIndex ───────────────────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_TaskOrderIndex_WhenNegative_FailsWithMinValueMessage()
    {
        var badTask = ValidTaskDto() with { OrderIndex = -1 };
        var command = ValidCreateCommand() with { Tasks = [badTask] };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Tasks[0].OrderIndex")
              .WithErrorMessage("OrderIndex must be 0 or greater.");
    }

    [Fact]
    public void CreateWorkflowCommandValidator_TaskOrderIndex_WhenZero_PassesValidation()
    {
        var task = ValidTaskDto() with { OrderIndex = 0 };
        var command = ValidCreateCommand() with { Tasks = [task] };
        var result = CreateValidator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("Tasks[0].OrderIndex");
    }

    // ── CreateWorkflow: Task AssigneeType ─────────────────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_TaskAssigneeType_WhenInvalidEnumValue_FailsWithEnumMessage()
    {
        var badTask = ValidTaskDto() with { AssigneeType = (AssigneeType)99 };
        var command = ValidCreateCommand() with { Tasks = [badTask] };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Tasks[0].AssigneeType")
              .WithErrorMessage("AssigneeType must be a valid value (Internal or External).");
    }

    // ── CreateWorkflow: Task NodeType ─────────────────────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_TaskNodeType_WhenInvalidEnumValue_FailsWithEnumMessage()
    {
        var badTask = ValidTaskDto() with { NodeType = (NodeType)99 };
        var command = ValidCreateCommand() with { Tasks = [badTask] };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Tasks[0].NodeType")
              .WithErrorMessage("NodeType must be a valid value.");
    }

    // ── CreateWorkflow: Task DefaultAssignedToEmail ───────────────────────────

    [Fact]
    public void CreateWorkflowCommandValidator_TaskDefaultAssignedToEmail_WhenExceeds256Characters_FailsWithMaxLengthMessage()
    {
        var badTask = ValidTaskDto() with { DefaultAssignedToEmail = new string('x', 257) };
        var command = ValidCreateCommand() with { Tasks = [badTask] };
        var result = CreateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Tasks[0].DefaultAssignedToEmail")
              .WithErrorMessage("DefaultAssignedToEmail must not exceed 256 characters.");
    }

    [Fact]
    public void CreateWorkflowCommandValidator_TaskDefaultAssignedToEmail_WhenNull_NoValidationError()
    {
        var task = ValidTaskDto() with { DefaultAssignedToEmail = null };
        var command = ValidCreateCommand() with { Tasks = [task] };
        var result = CreateValidator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("Tasks[0].DefaultAssignedToEmail");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UpdateWorkflowCommandValidator
    // ═══════════════════════════════════════════════════════════════════════════

    private static readonly UpdateWorkflowCommandValidator UpdateValidator = new();

    private static UpdateWorkflowCommand ValidUpdateCommand() =>
        new(
            Id: Guid.NewGuid(),
            Name: "Valid Workflow",
            Description: null,
            Category: null,
            IsActive: true,
            Tasks: [ValidTaskDto()]
        );

    // ── UpdateWorkflow: valid input ───────────────────────────────────────────

    [Fact]
    public void UpdateWorkflowCommandValidator_ValidCommand_NoValidationErrors()
    {
        var result = UpdateValidator.TestValidate(ValidUpdateCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── UpdateWorkflow: Id ────────────────────────────────────────────────────

    [Fact]
    public void UpdateWorkflowCommandValidator_Id_WhenEmptyGuid_FailsWithRequiredMessage()
    {
        var command = ValidUpdateCommand() with { Id = Guid.Empty };
        var result = UpdateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Workflow ID is required.");
    }

    // ── UpdateWorkflow: Name ──────────────────────────────────────────────────

    [Fact]
    public void UpdateWorkflowCommandValidator_Name_WhenEmpty_FailsWithRequiredMessage()
    {
        var command = ValidUpdateCommand() with { Name = "" };
        var result = UpdateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void UpdateWorkflowCommandValidator_Name_WhenExceeds200Characters_FailsWithMaxLengthMessage()
    {
        var command = ValidUpdateCommand() with { Name = new string('x', 201) };
        var result = UpdateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    // ── UpdateWorkflow: Description ───────────────────────────────────────────

    [Fact]
    public void UpdateWorkflowCommandValidator_Description_WhenExceeds2000Characters_FailsWithMaxLengthMessage()
    {
        var command = ValidUpdateCommand() with { Description = new string('x', 2001) };
        var result = UpdateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 2000 characters.");
    }

    // ── UpdateWorkflow: Category ──────────────────────────────────────────────

    [Fact]
    public void UpdateWorkflowCommandValidator_Category_WhenExceeds100Characters_FailsWithMaxLengthMessage()
    {
        var command = ValidUpdateCommand() with { Category = new string('x', 101) };
        var result = UpdateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Category)
              .WithErrorMessage("Category must not exceed 100 characters.");
    }

    // ── UpdateWorkflow: Tasks ─────────────────────────────────────────────────

    [Fact]
    public void UpdateWorkflowCommandValidator_Tasks_WhenNull_FailsWithNotNullMessage()
    {
        var command = ValidUpdateCommand() with { Tasks = null! };
        var result = UpdateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Tasks)
              .WithErrorMessage("Tasks must not be null.");
    }

    [Fact]
    public void UpdateWorkflowCommandValidator_TaskTitle_WhenEmpty_FailsWithRequiredMessage()
    {
        var badTask = ValidTaskDto() with { Title = "" };
        var command = ValidUpdateCommand() with { Tasks = [badTask] };
        var result = UpdateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Tasks[0].Title")
              .WithErrorMessage("Task title is required.");
    }

    [Fact]
    public void UpdateWorkflowCommandValidator_TaskOrderIndex_WhenNegative_FailsWithMinValueMessage()
    {
        var badTask = ValidTaskDto() with { OrderIndex = -1 };
        var command = ValidUpdateCommand() with { Tasks = [badTask] };
        var result = UpdateValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Tasks[0].OrderIndex")
              .WithErrorMessage("OrderIndex must be 0 or greater.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DeleteWorkflowCommandValidator
    // ═══════════════════════════════════════════════════════════════════════════

    private static readonly DeleteWorkflowCommandValidator DeleteValidator = new();

    // ── DeleteWorkflow: valid input ───────────────────────────────────────────

    [Fact]
    public void DeleteWorkflowCommandValidator_ValidCommand_NoValidationErrors()
    {
        var command = new DeleteWorkflowCommand(Guid.NewGuid());
        var result = DeleteValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── DeleteWorkflow: Id ────────────────────────────────────────────────────

    [Fact]
    public void DeleteWorkflowCommandValidator_Id_WhenEmptyGuid_FailsWithRequiredMessage()
    {
        var command = new DeleteWorkflowCommand(Guid.Empty);
        var result = DeleteValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Workflow ID is required.");
    }
}
