using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.DisableCredential;

public sealed record DisableApplicationCredentialCommand(
    Guid ApplicationId,
    Guid CredentialId)
    : IRequest;