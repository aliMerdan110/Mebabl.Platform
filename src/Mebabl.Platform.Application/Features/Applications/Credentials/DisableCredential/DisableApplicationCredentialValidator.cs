using FluentValidation;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.DisableCredential;

public sealed class DisableApplicationCredentialValidator
    : AbstractValidator<DisableApplicationCredentialCommand>
{
    public DisableApplicationCredentialValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.CredentialId).NotEmpty();
    }
}