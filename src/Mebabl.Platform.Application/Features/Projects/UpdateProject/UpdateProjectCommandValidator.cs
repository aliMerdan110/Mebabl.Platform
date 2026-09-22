
using FluentValidation;

namespace Mebabl.Platform.Application.Features.Projects.UpdateProject;

public sealed class UpdateProjectCommandValidator
    : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-zA-Z0-9_-]+$");

        RuleFor(x => x.Description)
            .MaximumLength(2000);
    }
}
