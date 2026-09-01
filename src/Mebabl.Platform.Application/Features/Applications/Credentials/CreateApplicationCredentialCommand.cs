using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.CreateCredential;

public sealed record CreateApplicationCredentialCommand(
    Guid ApplicationId)
    : IRequest<CreateApplicationCredentialResponse>;