using MediatR;
using Mebabl.Platform.Application.Features.Authentication.DTOs;

namespace Mebabl.Platform.Application.Features.Authentication.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthResponse>;