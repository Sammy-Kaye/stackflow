// DeleteWorkflowCommandValidator — validates the workflow ID is not an empty Guid.

using FluentValidation;

namespace StackFlow.Application.Features.Workflows.Commands.DeleteWorkflow;

public sealed class DeleteWorkflowCommandValidator : AbstractValidator<DeleteWorkflowCommand>
{
    public DeleteWorkflowCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Workflow ID is required.");
    }
}
