// GetWorkflowByIdQueryValidator — validates the workflow ID is not an empty Guid.
// The route constraint {id:guid} in the controller ensures the value is a valid Guid format,
// but FluentValidation catches the empty Guid edge case (00000000-0000-0000-0000-000000000000).

using FluentValidation;

namespace StackFlow.Application.Features.Workflows.Queries.GetWorkflowById;

public sealed class GetWorkflowByIdQueryValidator : AbstractValidator<GetWorkflowByIdQuery>
{
    public GetWorkflowByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Workflow ID is required.");
    }
}
