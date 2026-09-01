using FluentValidation;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.EnableCredential;

public sealed class EnableApplicationCredentialValidator
    : AbstractValidator<EnableApplicationCredentialCommand>
{
    public EnableApplicationCredentialValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.CredentialId).NotEmpty();
    }
}