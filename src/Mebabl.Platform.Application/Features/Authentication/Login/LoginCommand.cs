using MediatR;
using Mebabl.Platform.Application.Features.Authentication.DTOs;

namespace Mebabl.Platform.Application.Features.Authentication.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    Guid ApplicationId)
    : IRequest<AuthResponse>;