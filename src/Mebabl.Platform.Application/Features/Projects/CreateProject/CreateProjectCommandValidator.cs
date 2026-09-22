
using FluentValidation;

namespace Mebabl.Platform.Application.Features.Projects.CreateProject;

public sealed class CreateProjectCommandValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
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
