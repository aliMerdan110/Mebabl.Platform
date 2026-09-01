using MediatR;
using Mebabl.Platform.Application.Features.ApplicationAuthentication.DTOs;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Login;

public sealed record ApplicationLoginCommand(
    string ApiKey,
    string ApiSecret
)
: IRequest<ApplicationAuthResponse>;