using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.EnableCredential;

public sealed record EnableApplicationCredentialCommand(
    Guid ApplicationId,
    Guid CredentialId)
    : IRequest;