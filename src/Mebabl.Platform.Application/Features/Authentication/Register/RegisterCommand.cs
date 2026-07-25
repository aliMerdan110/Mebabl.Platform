using MediatR;
using Mebabl.Platform.Application.Features.Authentication.DTOs;

namespace Mebabl.Platform.Application.Features.Authentication.Register;

public sealed record RegisterCommand(
    string TenantName,
    string ApplicationName,
    string Email,
    string Username,
    string Password
) : IRequest<AuthResponse>;