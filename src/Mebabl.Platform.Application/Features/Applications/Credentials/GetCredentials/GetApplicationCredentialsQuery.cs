using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.GetCredentials;

public sealed record GetApplicationCredentialsQuery(
    Guid ApplicationId)
    : IRequest<IReadOnlyList<ApplicationCredentialResponse>>;