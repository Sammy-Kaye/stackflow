// UpdateWorkflowCommandValidator — validates all fields of UpdateWorkflowCommand.
// Runs in the ValidationBehavior pipeline step before the handler is called.
// Task validation mirrors CreateWorkflowCommandValidator.

using FluentValidation;

namespace StackFlow.Application.Features.Workflows.Commands.UpdateWorkflow;

public sealed class UpdateWorkflowCommandValidator : AbstractValidator<UpdateWorkflowCommand>
{
    public UpdateWorkflowCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Workflow ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters.")
            .When(x => x.Category is not null);

        RuleFor(x => x.Tasks)
            .NotNull().WithMessage("Tasks must not be null.");

        RuleForEach(x => x.Tasks).ChildRules(task =>
        {
            task.RuleFor(t => t.Title)
                .NotEmpty().WithMessage("Task title is required.")
                .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

            task.RuleFor(t => t.Description)
                .MaximumLength(2000).WithMessage("Task description must not exceed 2000 characters.")
                .When(t => t.Description is not null);

            task.RuleFor(t => t.AssigneeType)
                .IsInEnum().WithMessage("AssigneeType must be a valid value (Internal or External).");

            task.RuleFor(t => t.NodeType)
                .IsInEnum().WithMessage("NodeType must be a valid value.");

            task.RuleFor(t => t.OrderIndex)
                .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be 0 or greater.");

            task.RuleFor(t => t.DefaultAssignedToEmail)
                .MaximumLength(256).WithMessage("DefaultAssignedToEmail must not exceed 256 characters.")
                .When(t => t.DefaultAssignedToEmail is not null);
        });
    }
}
