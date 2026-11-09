using MediatR;

using Mebabl.Platform.Application.Features.ApplicationAuthentication.DTOs;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.TrustedLogin;

public sealed record ApplicationServerLoginCommand(
    string ApiKey,
    string ApiSecret
) : IRequest<ApplicationAuthResponse>;