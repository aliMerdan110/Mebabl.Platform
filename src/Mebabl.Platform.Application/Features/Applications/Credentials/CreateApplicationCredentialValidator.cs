using FluentValidation;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.CreateCredential;

public sealed class CreateApplicationCredentialValidator
    : AbstractValidator<CreateApplicationCredentialCommand>
{
    public CreateApplicationCredentialValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();
    }
}