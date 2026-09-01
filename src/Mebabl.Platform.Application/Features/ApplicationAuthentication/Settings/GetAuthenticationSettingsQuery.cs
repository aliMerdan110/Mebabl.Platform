using MediatR;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Settings;

public sealed record GetAuthenticationSettingsQuery(
    Guid ApplicationId
) : IRequest<AuthenticationSettingsResponse>;